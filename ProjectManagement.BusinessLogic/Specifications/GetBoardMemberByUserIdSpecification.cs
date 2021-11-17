using Ardalis.Specification;
using System.Linq;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Specifications
{
    public class GetBoardMemberByUserIdSpecification : Specification<BoardMember>
    {
        public int UserId { get; private set; }
        public int BoardId { get; private set; }
        public GetBoardMemberByUserIdSpecification(int userId, int boardId)
        {
            UserId = userId;
            BoardId = boardId;
            Query.Where(bm => bm.UserId == userId && bm.BoardId==boardId);
        }
    }
}
