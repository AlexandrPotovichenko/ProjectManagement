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
        Task<bool> UserExistsAsync(int userId);
        Task<User> GetForEditByIdAsync(int userId);

    }
}
