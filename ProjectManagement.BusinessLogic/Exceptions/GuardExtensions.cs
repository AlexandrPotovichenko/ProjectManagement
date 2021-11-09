
using ProjectManagement.Domain.Models;
using System.Linq;
using Ardalis.GuardClauses;
using System.Net;

namespace ProjectManagement.BusinessLogic.Exceptions
{
    public static class GuardExtensions
    {
        public static void NullObject(this IGuardClause guardClause, int objectId, object obj,string objName)
        {
            if (obj == null)
                throw new WebAppException((int)HttpStatusCode.NotFound, $"No {objName} found with id {objectId}");
        }

        public static void CheckMemebershipBoard(this IGuardClause guardClause, int UserId, Board board)
        {
            BoardMember boardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == UserId);
            if (boardMember != null)
                throw new WebAppException((int)HttpStatusCode.Conflict, $"a board memeber with this ID ({boardMember.Id}) already exists on the board ");
        }
        public static void CheckMemebershipCard(this IGuardClause guardClause, int UserId, Card card)
        {
            CardMember cardMember = card.CardMembers.FirstOrDefault(cm => cm.UserId == UserId);
            if (cardMember != null)
                throw new WebAppException((int)HttpStatusCode.Conflict, $"a card memeber with this ID ({cardMember.Id}) already exists on the board ");
        }

    }
}