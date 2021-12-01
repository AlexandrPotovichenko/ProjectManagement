using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Linq;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class UserRepository : BaseRepository<User, int, ProjectManagementContext>, IUserRepository
    {
        public UserRepository(ProjectManagementContext context) : base(context)
        {
        }
        public async Task<User> GetByNameAsync(string name)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Name == name);
        }
        public async Task<User> GetForEditByIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == userId);
        }
        public async Task<User> GetWithItemsByIdAsync(int userId)
        {
            return await _context.Users.Include(u=>u.Avatar).Where(x => x.Id == userId).FirstOrDefaultAsync();
        }
        public async Task<bool> IsUserExistsAsync(int userId)
        {
            return await _dbSet.AnyAsync(u => u.Id == userId);
        }
        public async Task<bool> IsUserExistsAsync(string userName)
        {
            return await _dbSet.AnyAsync(u => u.Name == userName);
        }
    }
}
