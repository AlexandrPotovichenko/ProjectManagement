using Ardalis.Specification;
using System.Linq;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Specifications
{
    public class GetBoardMemberByUserIdSpecification : Specification<BoardMember>
    {
        public GetBoardMemberByUserIdSpecification(int userId, int boardId)
        {
            Query.Where(bm => bm.UserId == userId && bm.BoardId==boardId);
        }
    }
}
