using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class CheckListRepository : BaseRepository<CheckList, int, ProjectManagementContext>, ICheckListRepository
    {
        public CheckListRepository(ProjectManagementContext context) : base(context)
        {
        }
        public async Task<CheckList> GetCheckListByCheckListItemId(int checkListItemId)
        {
            return await _context.CheckLists.Include(cl => cl.ChecklistItems).AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task<CheckList> GetWithItemsAsync(int checkListId)
        {
            return await _context.CheckLists.Where(cl => cl.Id == checkListId).Include(cl => cl.ChecklistItems).AsNoTracking().FirstOrDefaultAsync();
        }
    }
}
