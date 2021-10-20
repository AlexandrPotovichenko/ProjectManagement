
//using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace Ardalis.GuardClauses
{
    public static class BoardGuards
    {
        public static void NullObject(this IGuardClause guardClause, int ObjectId, object obj,string objName)
        {
            objName = obj.GetType().FullName;
            if (obj == null)
                throw new ObjectNotFoundException(ObjectId, objName);
        }

        public static void CheckMemebershipBoard(this IGuardClause guardClause, int UserId, Board board)
        {
            BoardMember boardMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == UserId);
            if (boardMember!=null)
                throw new MemberAlreadyExistsException($"a user with this ID ({boardMember.Id}) already exists on the board ", boardMember.Id);
        }
        public static void CheckMemebershipCard(this IGuardClause guardClause, int UserId, Card card)
        {
            CardMember cardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == UserId);
            if (cardMember != null)
                throw new MemberAlreadyExistsException($"a user with this ID ({cardMember.Id}) already exists on the board ", cardMember.Id);
        }
 
        //public static void MemberAlreadyExists(this IGuardClause guardClause, IReadOnlyCollection<BasketItem> basketItems)
        //{
        //    if (!basketItems.Any())
        //        throw new EmptyBasketOnCheckoutException();
        //}
    }
}