using System;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

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
                throw new System.Exception();// гозирага
            }

            //if (login == "John Doe")
            //{
            //    return Task.FromResult(new User
            //    {
            //        Id = 1,
            //        Name = login
            //    });
            //}

            //return Task.FromResult((User)null);
            return user;
        }

        public Task<User> RegisterUserAsync(string login, string password)
        {
            throw new NotImplementedException();
        }
    }
}
