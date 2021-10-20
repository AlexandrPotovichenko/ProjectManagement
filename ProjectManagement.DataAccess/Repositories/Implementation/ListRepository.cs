using Microsoft.EntityFrameworkCore;
using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class ListRepository : BaseRepository<List, int, ProjectManagementContext>, IListRepository
    {
        public ListRepository(ProjectManagementContext context) : base(context)
        {
        }
        //public async Task<IEnumerable<List>> GetWithItemsAsync(int userId)
        //{
        //    return await _context.Cards.Where(c => c.CardMembers.Any(cm => cm.UserId == userId)).Include(c => c.CardMembers).Include(c => c.CheckLists).Include(c => c.Actions).ToListAsync<Card>();
        //}
    }
}
