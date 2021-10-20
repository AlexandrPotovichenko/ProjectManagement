using Ardalis.GuardClauses;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Specifications;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System.Linq;
using System;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IListRepository _listRepository;
        private readonly ICardMemberRepository _cardMemberRepository;
        private readonly IUserManager _userManager;
        public CardService(ICardRepository cardRepository,  IListRepository listRepository, ICardMemberRepository cardMemberRepository, IUserManager userManager)
        {
            _cardRepository = cardRepository;
            _listRepository = listRepository;
            _cardMemberRepository = cardMemberRepository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Card>> GetCardsAsync()
        {
            int currentUserId = _userManager.GetCurrentUserId();
            return await _cardRepository.GetWithItemsAsync(currentUserId);
        }

        public async Task<Card> CreateCardAsync(string name, string description, int listId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            List list = await GetListByIdAsync(listId);
            bool userCanCreateCard = await _cardRepository.CanCreateCardAsync(list.BoardId, currentUserId);

            if (!userCanCreateCard)
            {
                throw new Exception();
            }

            CardMember CardMember = new CardMember(currentUserId, Role.Admin);
            Card card = new Card(name, description, CardMember, listId);

            var insertedItem = await _cardRepository.InsertAsync(card);

            await _cardRepository.UnitOfWork.SaveChangesAsync();

            string actionDescription = $"Create Card";
            insertedItem.Actions.Add(new CardAction(CardMember, description));
            await _cardRepository.UpdateAsync(card);

            await _cardRepository.UnitOfWork.SaveChangesAsync();

            return insertedItem;
        }

        public async Task<Card> GetCardAsync(int cardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            return await _cardRepository.GetByIdAsync(cardId);
        }

        public async Task DeleteCardAsync(int cardId)
        {

            int currentUserId = _userManager.GetCurrentUserId();
            CardMember cardMember = await GetMemberByUserIdAsync(cardId, currentUserId);
            if (!cardMember.IsMemberAdmin)
            {
                throw new Exception();
            }

            await _cardRepository.DeleteByIdAsync(cardId);
        }

        public async Task<IEnumerable<CardMember>> GetMembershipOfMemberOnCardAsync(int cardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            CardMember cardMember = await GetMemberByUserIdAsync(cardId, currentUserId);
            if (!cardMember.CanRead)
            {
                throw new Exception();
            }

            Card card = await GetCardByIdAsync(cardId);

            return card.CardMembers;
        }

        public async Task<CardMember> AddMemberToCardAsync(int userId, int cardId, Role role)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            User userForMembership = await _userManager.GetUserByIdAsync(userId);
            Card card = await GetCardByIdAsync(cardId);

            CardMember cardMember = await GetMemberByUserIdAsync(cardId, currentUserId);
            if (!cardMember.IsMemberAdmin)
            {
                throw new Exception();
            }

            CardMember newCardMember = new CardMember(userId, role);

            card.CardMembers.Add(newCardMember);
            
            string actionDescription = $"Add member {userForMembership.Name}({userForMembership.Id}) with role {role}";
            CardAction cardAction = new CardAction(cardMember, actionDescription);
            card.Actions.Add(cardAction);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            return cardMember;
        }

        private async Task<CardMember> GetMemberByUserIdAsync(int cardId, int userId)
        {
            var memberSpec = new GetCardMemberByUserIdSpecification(userId, cardId);
            CardMember  cardMember = await _cardMemberRepository.GetSingleAsync(memberSpec);
            Guard.Against.NullObject(userId, cardMember, "CardMember");

            return cardMember;
        }

        public async Task RemoveMemberFromCardAsync(int memberId, int cardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            Card card = await GetCardByIdAsync(cardId);
            CardMember сardMemberForRemoveFromMembership =  GetCardMemberByMemberId(card,memberId);
           User userForRemoveFromMembership = await _userManager.GetUserByIdAsync(сardMemberForRemoveFromMembership.UserId);

            CardMember currentCardMember = await GetMemberByUserIdAsync(cardId, currentUserId);
            if (!currentCardMember.IsMemberAdmin)
            {
                throw new Exception();
            }

            CardMember сardMember = GetCardMemberByMemberId(card, memberId);

            card.CardMembers.Remove(сardMember);

            string actionDescription = $"Remove user {userForRemoveFromMembership.Name}({userForRemoveFromMembership.Id}) from membership";
            CardAction cardAction = new CardAction(currentCardMember, actionDescription);
            card.Actions.Add(cardAction);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateMembershipOfMemberOnCardAsync(int cardId, int memberId, Role newRole)
        {

            int currentUserId = _userManager.GetCurrentUserId();
            Card card = await GetCardByIdAsync(cardId);
            CardMember currentCardMember = await GetMemberByUserIdAsync(cardId, currentUserId);
            if (!currentCardMember.IsMemberAdmin)
            {
                throw new Exception();
            }

            CardMember cardMember = GetCardMemberByMemberId(card, memberId);

            User user = await _userManager.GetUserByIdAsync(cardMember.UserId);
            Guard.Against.NullObject(cardMember.UserId, user, "User");
            Role oldRole = cardMember.Role;
            cardMember.Role = newRole;

            string description = $"Update membership for user {user.Name}({user.Id}) from {oldRole} to {newRole}";
            card.Actions.Add(new CardAction(currentCardMember, description));

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task AddNewCommentToCardAsync(int cardId, string comment)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            CardMember currentCardMember = await GetMemberByUserIdAsync(cardId, currentUserId);
            if (!currentCardMember.CanUpdate)
            {
                throw new Exception();
            }

            Card card = await GetCardByIdAsync(cardId);
            User user = await _userManager.GetUserByIdAsync(currentUserId);

            CardMember cardMember = GetCardMemberByUserId(card, currentUserId);
            CardAction cardAction = new CardAction(cardMember, comment, true);

            card.Actions.Add(cardAction);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();

        }

        public async Task<IEnumerable<CardAction>> GetCommentsOnCardAsync(int cardId)
        {
            Card card = await GetCardByIdAsync(cardId);
            return card.Actions.Where(a=>a.IsComment);
        }

        public async Task DeleteCommentOnCardAsync(int cardId, int commentId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            CardMember currentCardMember = await GetMemberByUserIdAsync(cardId, currentUserId);
            if (!currentCardMember.CanUpdate)
            {
                throw new Exception();
            }          

            Card card = await GetCardByIdAsync(cardId);

            CardAction cardAction = card.Actions.FirstOrDefault(a => a.Id == commentId);
            Guard.Against.NullObject(commentId, cardAction, "Comment");

            if(cardAction.Member!=currentCardMember) // the user can delete his own comments
            {
                throw new Exception();
            }

            card.Actions.Remove(cardAction);

            string description = $"Delete Comment";
            card.Actions.Add(new CardAction(currentCardMember, description));

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task MoveCardToListAsync(int cardId, int newListId)
        {

            int currentUserId = _userManager.GetCurrentUserId();
            CardMember currentCardMember = await GetMemberByUserIdAsync(cardId, currentUserId);
            if (!currentCardMember.CanUpdate)
            {
                throw new Exception();
            }

            Card card = await GetCardByIdAsync(cardId);
            List newList = await GetListByIdAsync(newListId);
            var oldList = card.List;
            card.List = newList;

            string description = $"Move Card from list {oldList.Name} to list {newList}";
            card.Actions.Add(new CardAction(currentCardMember, description));

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        private async Task<Card> GetCardByIdAsync(int cardId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            return card;
        }

        private async Task<List> GetListByIdAsync(int listId)
        {
            List list = await _listRepository.GetByIdAsync(listId);
            Guard.Against.NullObject(listId, list, "List");
            return list;
        }
        private CardMember GetCardMemberByMemberId(Card card, int cardMemberId)
        {
            CardMember cardMember = card.CardMembers.FirstOrDefault(cm => cm.Id == cardMemberId);
            Guard.Against.NullObject(cardMemberId, cardMember, "CardMember");
            return cardMember;
        }
        private CardMember GetCardMemberByUserId(Card card, int userId)
        {
            CardMember cardMember = card.CardMembers.FirstOrDefault(cm => cm.UserId == userId);
            Guard.Against.NullObject(userId, cardMember, "CardMember");
            return cardMember;
        }
        public async Task<Role> GetMemberRoleAsync(int userId, int cardId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            CardMember cardMember = GetCardMemberByUserId(card, userId);
            return cardMember.Role;
        }  
    }
}
