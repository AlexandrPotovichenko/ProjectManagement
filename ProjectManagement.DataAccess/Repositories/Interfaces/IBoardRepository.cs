using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.DataAccess.Repositories.Interfaces
{
    public interface IBoardRepository : IRepository<Board, int>
    {
        Task<IEnumerable<Board>> GetWithItemsAsync(int userId);
        Task<Board> GetWithItemsByIdAsync(int boardId);
        Task<Board> GetWithMembersAsync(int boardId);
        Task<Board> GetForEditByIdAsync(int boardId);
        Task<BoardMember> GetBoardMemberByIdAsync(int boardMemberId);
    }
}
