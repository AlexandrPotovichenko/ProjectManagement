using Ardalis.GuardClauses;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Specifications;
using ProjectManagement.BusinessLogic.Services.Interfaces;

using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Linq;
using System;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IListRepository _listRepository;
        public CardService(ICardRepository cardRepository, IListRepository  listRepository,IUserRepository userRepository)
        {
            _cardRepository = cardRepository;
            _userRepository = userRepository;
            _listRepository = listRepository;
        }

        public async Task<IEnumerable<Card>> GetCardsAsync()
        {
            return await _cardRepository.GetAllAsync();
        }

        public async Task<Card> CreateCardAsync( string name, string description, int listId, int userId)
        {

            User user = await _userRepository.GetByIdAsync(userId);
            Guard.Against.NullObject(userId, user, "User");

            List list = await _listRepository.GetByIdAsync(listId);
            Guard.Against.NullObject(listId, list, "List");

            CardMember CardMember = new CardMember(userId, Role.Admin);
            
            var item = new Card(name, description, CardMember,listId);
            var insertedItem = await _cardRepository.InsertAsync(item);
            await _cardRepository.UnitOfWork.SaveChangesAsync();

            return insertedItem;
        }

        public async Task<Card> GetCardAsync(int cardId)
        {
            return await _cardRepository.GetByIdAsync(cardId);
        }

        public async Task DeleteCard(int cardId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");

            await _cardRepository.DeleteByIdAsync(cardId);
        }

        public async Task<IEnumerable<CardMember>> GetMembershipOfMemberOnCard(int cardId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");

            return card.CardMembers;
        }

        public async Task AddMemberToCard(int userId, int cardId, Role role)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");

            User user = await _userRepository.GetByIdAsync(userId);
            Guard.Against.NullObject(userId, user, "User");
            Guard.Against.CheckMemebershipCard(userId, card);

            CardMember cardMember = new CardMember(userId, role);
            card.CardMembers.Add(cardMember);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task RemoveMemberFromCard(int memberId, int cardId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");

            CardMember boardMember = card.CardMembers.FirstOrDefault(bm => bm.Id == memberId);
            Guard.Against.NullObject(memberId, boardMember, "CardMember");

            card.CardMembers.Remove(boardMember);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateMembershipOfMemberOnCard(int cardId, int memberId, Role newRole)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");

            CardMember cardMember = card.CardMembers.FirstOrDefault(bm => bm.Id == memberId);
            Guard.Against.NullObject(memberId, cardMember, "CardMember");

            cardMember.Role = newRole;

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task AddNewCommentToCard(int cardId,int userId, string comment)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            User user = await _userRepository.GetByIdAsync(userId);
            Guard.Against.NullObject(userId, user, "User");

            CardAction cardAction = new CardAction();
            cardAction.IsComment = true;
            cardAction.Date = DateTime.Now;
            cardAction.Member = card.CardMembers.FirstOrDefault(cm => cm.UserId == userId);

            cardAction.Description = comment;

        }

        public async Task<IEnumerable<CardAction>> GetCommentsOnCard(int cardId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            return card.Actions;
        }

        public async Task DeleteCommentOnCard(int cardId, int commentId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");

            CardAction cardAction = card.Actions.FirstOrDefault(a => a.Id == commentId);
            Guard.Against.NullObject(commentId, cardAction, "Comment");

        }

        public async Task MoveCardToList(int cardId, int newListId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            List list = await _listRepository.GetByIdAsync(newListId);
            Guard.Against.NullObject(newListId, list, "List");
            card.List = list;
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }


    }
}
