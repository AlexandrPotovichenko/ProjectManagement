using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class UserMananger: IUserManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        public UserMananger(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }
        public int GetCurrentUserId()
        {
            HttpContext context = _httpContextAccessor.HttpContext;
            int userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return userId;
        }
        public async Task<User> GetCurrentUserAsync()
        {
            int currentUserId = GetCurrentUserId();
            return await GetUserByIdAsync(currentUserId);
        }
        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _userRepository.UserExistsAsync(userId);       
        }
        public async Task<User> GetUserByIdAsync(int userId)
        {
            User user = await _userRepository.GetByIdAsync(userId);
            Guard.Against.NullObject(userId, user, "User");
            return user;
        }
    }
}

