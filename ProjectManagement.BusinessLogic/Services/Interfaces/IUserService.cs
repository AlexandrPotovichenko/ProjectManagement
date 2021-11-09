using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> AuthenticateUserAsync(string login, string password);
        Task<User> RegisterUserAsync(string login, string password);
        Task ChangePasswordAsync(string login, string password, string newPassword);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<AppFile> DownloadAvatarAsync(int userId);
        Task UploadAvatarAsync(int userId, IFormFile file);
    }
}
