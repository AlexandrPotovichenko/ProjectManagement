
using ProjectManagement.Domain.Models;
using System.Linq;
using Ardalis.GuardClauses;

namespace ProjectManagement.BusinessLogic.Exceptions
{
    public static class GuardExtensions
    {
        public static void NullObject(this IGuardClause guardClause, int ObjectId, object obj,string objName)
        {
            if (obj == null)
                throw new ObjectNotFoundException(ObjectId, objName);
        }

        public static void CheckMemebershipBoard(this IGuardClause guardClause, int UserId, Board board)
        {
            BoardMember boardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == UserId);
            if (boardMember!=null)
                throw new MemberAlreadyExistsException($"a board memeber with this ID ({boardMember.Id}) already exists on the board ", boardMember.Id);
        }
        public static void CheckMemebershipCard(this IGuardClause guardClause, int UserId, Card card)
        {
            CardMember cardMember = card.CardMembers.FirstOrDefault(cm => cm.UserId == UserId);
            if (cardMember != null)
                throw new MemberAlreadyExistsException($"a card memeber with this ID ({cardMember.Id}) already exists on the board ", cardMember.Id);
        }

    }
}