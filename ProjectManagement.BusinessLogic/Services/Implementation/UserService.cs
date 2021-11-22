using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;
        public UserService(IUserRepository userRepository, IFileService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
        }
        public async Task<User> AuthenticateUserAsync(string login, string password)
        {
            User user = await _userRepository.GetByNameAsync(login);
            if (user is null)
            {
                throw new System.Exception();
            }
            return user;
        }

        public async Task ChangePasswordAsync(string login, string password, string newPassword)
        {
            User user = await AuthenticateUserAsync(login, password);
            User userForEdit = await _userRepository.GetForEditByIdAsync(user.Id);
            userForEdit.PasswordHash = BCryptNet.HashPassword(newPassword);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> RegisterUserAsync(string login, string password)
        {
            string passwordHash = BCryptNet.HashPassword(password);
            User user = new User(login, passwordHash);
            user = await _userRepository.InsertAsync(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return user;
        }
        public async Task UploadAvatarAsync(int userId, IFormFile file)
        {
            _fileService.CheckFileForAvatar(file); // checking file size and extension 
            await _fileService.ScanFileForVirusesAsync(file);
            User user = await _userRepository.GetForEditByIdAsync(userId);
            MemoryStream ms = new MemoryStream();
            // Copy the file to the MemoryStream
            await file.CopyToAsync(ms);
            AppFile appFile = new AppFile(file.FileName,file.ContentType, ms.ToArray());
            user.Avatar = appFile;
            await _userRepository.UpdateAsync(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
        }
        public async Task<AppFile> DownloadAvatarAsync(int userId)
        {
            User user = await _userRepository.GetWithItemsByIdAsync(userId);
            if(user==null)
            {
                string message = $"User with ID {userId} not found";
                throw new WebAppException((int)HttpStatusCode.NotFound, message);
            }
                
            return user.Avatar;
        }
    }
}
