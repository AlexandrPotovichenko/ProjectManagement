using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface ICardService
    {
        Task<IEnumerable<Card>> GetCardsAsync();
        Task<Card> CreateCardAsync(string name, string description, int listId);
        Task<Card> GetCardAsync(int cardId);
        Task DeleteCardAsync(int cardId);
        Task<IEnumerable<CardMember>> GetMembershipOfMemberOnCardAsync(int cardId);
        Task<CardMember> AddMemberToCardAsync(int newMemberUserId, int cardId, Role role);
        Task RemoveMemberFromCardAsync(int memberId, int cardId);
        Task UpdateMembershipOfMemberOnCardAsync(int cardId, int memberId, Role newRole);
        Task<CardAction> AddCommentToCardAsync(int cardId , string comment);
        Task<IEnumerable<CardAction>> GetCommentsOnCardAsync(int cardId);
        Task<IEnumerable<CardAction>> GetCommentOnCardAsync(int cardId,int commentId);
        Task DeleteCommentOnCardAsync(int cardId, int commentId);
        Task MoveCardToListAsync(int cardId, int newListId);
    }
}