using Ardalis.Specification;
using System.Linq;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Specifications
{
    public class GetCardMemberByUserIdSpecification : Specification<CardMember>
    {
        public int UserId { get; private set; }
        public int CardId { get; private set; }
        public GetCardMemberByUserIdSpecification(int userId, int cardId)
        {
            UserId = userId;
            CardId = cardId;
            Query.Where(cm => cm.UserId == userId && cm.CardId==cardId);
        }
    }
}
