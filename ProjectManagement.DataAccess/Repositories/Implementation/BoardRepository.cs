using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class BoardRepository : BaseRepository<Board, int, ProjectManagementContext>, IBoardRepository
    {
        public BoardRepository(ProjectManagementContext context) : base(context)
        {

        }
        public async Task<IEnumerable<Board>> GetWithItemsAsync(int userId)
        {
 
            return await _context.Boards.Where(b => b.BoardMembers.Any(bm => bm.UserId == userId)).Include(b => b.BoardMembers).Include(b => b.Lists).ThenInclude(l => l.Cards).ToListAsync<Board>();

        }
        public async Task<Board> GetWithMembersAsync(int boardId)
        {

            return await _context.Boards.Include(b => b.BoardMembers).FirstOrDefaultAsync();

        }
        //public Task<bool> UserCanReadAsync(int boardId, int userId)
        //{
        //     return (await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId))?.BoardMembers.Any(bm => bm.UserId == userId);

        //    .Any(b=>b.BoardMembers.Any(bm=>bm.UserId==userId)
        //   throw new System.NotImplementedException();
        //}

        //public Task<bool> UserCanUpdateAsync(int boardId, int userId)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}
