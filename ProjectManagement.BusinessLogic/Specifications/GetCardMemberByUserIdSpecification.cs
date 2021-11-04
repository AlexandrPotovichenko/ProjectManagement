using Ardalis.Specification;
using System.Linq;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Specifications
{
    public class GetCardMemberByUserIdSpecification : Specification<CardMember>
    {
        public GetCardMemberByUserIdSpecification(int userId, int boardId)
        {
            Query.Where(cm => cm.UserId == userId && cm.CardId==boardId);
        }
    }
}
