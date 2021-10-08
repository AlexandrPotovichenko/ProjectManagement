using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface IBoardService
    {

       
        Task<Board> CreateBoardAsync(string name, string description, int userId);
        Task<Board> GetBoardAsync(int boardId);
        Task<IEnumerable<Board>> GetBoardsAsync();
        Task DeleteBoard(int boardId);

        Task<IEnumerable<BoardMember>> GetMembershipOfMemberOnBoard(int boardId);
        Task AddMemberToBoard(int userId, int boardId,Role role);
        Task RemoveMemberFromBoard(int memberId, int boardId);
        Task UpdateMembershipOfMemberOnBoard(int boardId, int memberId, Role newRole);

    }
}