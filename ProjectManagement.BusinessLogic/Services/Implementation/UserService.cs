using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
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
        private readonly IBoardMemberRepository _boardMemberRepository;
        private readonly ICardMemberRepository _cardMemberRepository;
        private readonly IFileService _fileService;
        public UserService(IUserRepository userRepository, IBoardMemberRepository boardMemberRepository, ICardMemberRepository cardMemberRepository, IFileService fileService)
        {
            _userRepository = userRepository;
            _boardMemberRepository = boardMemberRepository;
            _cardMemberRepository = cardMemberRepository;
            _fileService = fileService;
        }
        public async Task<User> AuthenticateUserAsync(string login, string password)
        {
            User user = await _userRepository.GetByNameAsync(login);
            if (user is null || !BCryptNet.Verify(password, user.PasswordHash))
            {
                throw new WebAppException((int)HttpStatusCode.Unauthorized, "Login or password is incorrect.");
            }         
            return user;
        }
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }
        public async Task<User> RegisterUserAsync(string login, string password)
        {
            CheckLogin(login);
            string passwordHash = BCryptNet.HashPassword(password);
            User user = new User(login, passwordHash);
            user = await _userRepository.InsertAsync(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
            return user;
        }
        public async Task UpdateUserAsync(int userId,string name, string password)
        {
            User user = await _userRepository.GetForEditByIdAsync(userId);
            if(user == null)
            {
                throw new WebAppException((int)HttpStatusCode.Unauthorized, "Login or password is incorrect.");
            }
            CheckLogin(name);
            user.Name = name;
            string passwordHash = BCryptNet.HashPassword(password);
            user.PasswordHash = passwordHash;
            await _userRepository.UpdateAsync(user);
            await _userRepository.UnitOfWork.SaveChangesAsync();
        }
        public async Task DeleteUserAsync(int userId)
        {
            User user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new WebAppException((int)HttpStatusCode.NotFound, $"A user with ID:{userId} do not exists!");
            }
            BoardMember boardMember =await _boardMemberRepository.GetByIdAsync(userId);
            if(boardMember!=null)
            {
                throw new WebAppException((int)HttpStatusCode.Conflict, $"user with ID:{userId} is a member on the board with ID:{boardMember.BoardId}");
            }
            CardMember cardMember = await _cardMemberRepository.GetByIdAsync(userId);
            if (cardMember != null)
            {
                throw new WebAppException((int)HttpStatusCode.Conflict, $"user with ID:{userId} is a member on the card with ID:{cardMember.CardId}");
            }
            await _userRepository.DeleteByIdAsync(userId);
        }
        public async Task UploadAvatarAsync(int userId, IFormFile file)
        {
            User user = await _userRepository.GetForEditByIdAsync(userId);
            Guard.Against.NullObject(userId, user, "User");
            _fileService.CheckFileForAvatar(file); // checking file size and extension 
            await _fileService.ScanFileForVirusesAsync(file); 
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
            Guard.Against.NullObject(userId, user, "User");
            if (user==null)
            {
                string message = $"User with ID {userId} not found";
                throw new WebAppException((int)HttpStatusCode.NotFound, message);
            }           
            return user.Avatar;
        }
        private async void CheckLogin(string login)
        {
            bool loginAlreadyExists = await _userRepository.IsUserExistsAsync(login);
            if (loginAlreadyExists)
            {
                throw new WebAppException((int)HttpStatusCode.Conflict, "A user with this login already exists!");
            }
        }
    }
}
