using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface IBoardService
    {
        Task<Board> CreateBoardAsync(string name, string description);
        Task<Board> GetBoardAsync(int boardId);
        Task<IEnumerable<Board>> GetBoardsAsync();
        Task DeleteBoardAsync(int boardId);

        Task<IEnumerable<BoardMember>> GetMembershipOfMemberOnBoardAsync(int boardId);
        Task<BoardMember> AddMemberToBoardAsync(int newMemberUserId, int boardId,Role role);
        Task RemoveMemberFromBoardAsync(int boardId, int memberId);
        Task UpdateMembershipOfMemberOnBoardAsync(int boardId, int memberId, Role newRole);

    }
}