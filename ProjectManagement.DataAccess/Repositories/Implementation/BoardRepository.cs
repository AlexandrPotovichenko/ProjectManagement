using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Collections.Generic;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class BoardRepository : BaseRepository<Board, int, ProjectManagementContext>, IBoardRepository
    {
        public BoardRepository(ProjectManagementContext context) : base(context)
        {

        }
        public async Task<IEnumerable<Board>> GetWithItemsAsync()
        {
            return await  _context.Boards.Include(b=>b.BoardMembers)
 .Include(b => b.Lists).ThenInclude(l=>l.Cards).ToListAsync<Board>() ;
        }
 

 //           public async Task<IEnumerable<Board>> DeleteAsync(int boardId)
 //       {
 //           return await _context.Boards..Include(b => b.BoardMembers)
 //.Include(b => b.Lists).ThenInclude(l => l.Cards).ToListAsync<Board>();
 //       }
    }
}
