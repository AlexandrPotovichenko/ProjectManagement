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
        public async Task<List> GetForEditByIdAsync(int listId)
        {
            return await _context.Lists.Where(l => l.Id == listId).Include(b => b.Cards).FirstOrDefaultAsync();
        }
    }
}
