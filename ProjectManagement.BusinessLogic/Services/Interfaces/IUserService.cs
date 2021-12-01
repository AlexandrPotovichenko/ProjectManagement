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
        Task UpdateUserAsync(int userId,string login, string password);
        Task DeleteUserAsync(int userId);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<AppFile> DownloadAvatarAsync(int userId);
        Task UploadAvatarAsync(int userId, IFormFile file);
    }
}
