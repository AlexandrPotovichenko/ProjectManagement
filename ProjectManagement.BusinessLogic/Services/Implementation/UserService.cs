using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _UserRepository;

         public UserService(IUserRepository userRepository)
        {
            _UserRepository = userRepository;
        }
        public async Task<User> AuthenticateUserAsync(string login, string password)
        {
            var user = await _UserRepository.GetByNameAsync(login);
            if (user is null)
            {
                throw new System.Exception();
            }
            return user;
        }

        public async Task ChangePasswordAsync(string login, string password, string newPassword)
        {
            var user = AuthenticateUserAsync(login, password);
            User userForEdit = await _UserRepository.GetForEditByIdAsync(user.Id);
            userForEdit.PasswordHash = BCryptNet.HashPassword(newPassword);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _UserRepository.GetAllAsync();
        }

        public async Task<User> RegisterUserAsync(string login, string password)
        {
            string passwordHash = BCryptNet.HashPassword(password);
            User user = new User(login, passwordHash);
            user = await _UserRepository.InsertAsync(user);
            await _UserRepository.UnitOfWork.SaveChangesAsync();
            return user;
        }
    }
}
