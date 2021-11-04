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
            return await _context.Boards.Where(b => b.BoardMembers.Any(bm => bm.UserId == userId)).Include(b => b.BoardMembers).Include(b => b.Lists).ThenInclude(l => l.Cards).AsNoTracking().ToListAsync<Board>();
        }
        public async Task<Board> GetWithItemsByIdAsync(int boardId)
        {
            return await _context.Boards.Where(b => b.Id == boardId).Include(b => b.BoardMembers).Include(b => b.Lists).ThenInclude(l => l.Cards).AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task<Board> GetWithMembersAsync(int boardId)
        {
            return await _context.Boards.Include(b => b.BoardMembers).AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task<Board> GetForEditByIdAsync(int boardId)
        {
            return await _context.Boards.Where(b => b.Id == boardId).Include(b => b.BoardMembers).Include(b => b.Lists).ThenInclude(l => l.Cards).FirstOrDefaultAsync();
        }
        public async Task<BoardMember> GetBoardMemberByIdAsync(int boardMemberId)
        {
            return await _context.Boards.Where(b => b.BoardMembers.Any(bm => bm.Id == boardMemberId)).Select(b => b.BoardMembers.FirstOrDefault(bm => bm.Id == boardMemberId)).AsNoTracking().FirstOrDefaultAsync();
        }
    }
}
