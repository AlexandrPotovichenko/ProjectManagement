using ProjectManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface IUserManager
    {
        int GetCurrentUserId();
        Task<User> GetCurrentUserAsync();
        Task<bool> UserExistsAsync(int userId);
        Task<User> GetUserByIdAsync(int userId);
    }
}
