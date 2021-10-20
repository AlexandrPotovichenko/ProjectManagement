using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class UserRepository : BaseRepository<User, int, ProjectManagementContext>, IUserRepository
    {
        public UserRepository(ProjectManagementContext context) : base(context)
        {
        }

        public async Task<User> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _dbSet.AnyAsync(u => u.Id == userId);
        }
        //public async Task<string> GetUserByIdAsync(int userId)
        //{
        //    string name = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
        //    return
        //}
        
    }
}
