using Microsoft.EntityFrameworkCore;
using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class CardRepository : BaseRepository<Card, int, ProjectManagementContext>, ICardRepository
    {
        public CardRepository(ProjectManagementContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Card>> GetWithItemsAsync(int userId)
        {
            return await _context.Cards.Where(c => c.CardMembers.Any(cm => cm.UserId == userId)).Include(c => c.CardMembers).Include(c=>c.CheckLists).Include(c => c.Actions).Include(c => c.List).AsNoTracking().ToListAsync<Card>();
        }

        public async Task<Card> GetForEditByIdAsync(int cardId)
        {
            return await _context.Cards.Where(c => c.Id == cardId).Include(c => c.CardMembers).Include(c => c.CheckLists).ThenInclude(cl=>cl.ChecklistItems).Include(c => c.Actions).Include(c=>c.List).FirstOrDefaultAsync(); 
        }

        public async Task<Card> GetWithMembersAsync(int cardId)
        {
            return await _context.Cards.Where(c => c.Id == cardId).Include(b => b.CardMembers).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<bool> CanCreateCardAsync(int boardId, int userId)
        {
            BoardMember boardMember = await _context.BoardMembers.Where(bm => bm.BoardId == boardId && bm.UserId == userId).AsNoTracking().FirstOrDefaultAsync();
            if (boardMember == null) // there is no member on the board with this ID
                return false;
            return boardMember.CanUpdate;

        }
        public async Task<Card> GetWithItemsByIdAsync(int cardId)
        {
            return await _context.Cards.Where(c => c.Id == cardId).Include(c => c.CardMembers).Include(c => c.CheckLists).Include(c => c.Actions).Include(c => c.List).AsNoTracking().FirstOrDefaultAsync();
        }
    }
}
