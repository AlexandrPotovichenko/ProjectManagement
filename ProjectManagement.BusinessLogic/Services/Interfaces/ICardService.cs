using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface ICardService
    {
        Task<IEnumerable<Card>> GetCardsAsync();
        Task<Card> CreateCardAsync(string name, string description, int listId,int userId);

        Task<Card> GetCardAsync(int cardId);
        Task DeleteCard(int cardId);


        Task<IEnumerable<CardMember>> GetMembershipOfMemberOnCard(int cardId);
        Task AddMemberToCard(int userId, int cardId, Role role);
        Task RemoveMemberFromCard(int memberId, int cardId);
        Task UpdateMembershipOfMemberOnCard(int cardId, int memberId, Role newRole);
        Task AddNewCommentToCard(int cardId , int userId, string comment);
        Task<IEnumerable<CardAction>> GetCommentsOnCard(int cardId);
        Task DeleteCommentOnCard(int cardId, int commentId);
        Task MoveCardToList(int cardId, int newListId);


    }
}