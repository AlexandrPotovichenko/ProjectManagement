using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.DataAccess.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User, int>
    {
        Task<User> GetByNameAsync(string name);
        Task<bool> IsUserExistsAsync(int userId);
        Task<bool> IsUserExistsAsync(string userName);
        Task<User> GetForEditByIdAsync(int userId);
        Task<User> GetWithItemsByIdAsync(int userId);
    }
}
