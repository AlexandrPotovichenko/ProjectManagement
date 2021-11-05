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
using System.Net;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IListRepository _listRepository;
        private readonly ICardMemberRepository _cardMemberRepository;
        private readonly IUserManager _userManager;
        public CardService(ICardRepository cardRepository, IListRepository listRepository, ICardMemberRepository cardMemberRepository, IUserManager userManager)
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
            List list = await _listRepository.GetForEditByIdAsync(listId);
            bool userCanCreateCard = await _cardRepository.CanCreateCardAsync(list.BoardId, currentUserId);
            if (!userCanCreateCard)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            CardMember CardMember = new CardMember(currentUserId, Role.Admin);
            Card card = new Card(name, description, CardMember, listId);
            card.List = list;
            string actionDescription = $"Create Card";
            card.Actions.Add(new CardAction(CardMember.Id, actionDescription));
            Card insertedItem = await _cardRepository.InsertAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            return insertedItem;
        }

        public async Task<Card> GetCardAsync(int cardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            return await _cardRepository.GetWithItemsByIdAsync(cardId);
        }

        public async Task DeleteCardAsync(int cardId)
        {
            CardMember cardMember = await GetCurrentCardMemberAsync(cardId);
            if (cardMember != null && !cardMember.IsMemberAdmin)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            await _cardRepository.DeleteByIdAsync(cardId);
        }

        public async Task<IEnumerable<CardMember>> GetMembershipOfMemberOnCardAsync(int cardId)
        {
            CardMember cardMember = await GetCurrentCardMemberAsync(cardId);
            if (cardMember != null && !cardMember.CanRead)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Card card = await GetCardByIdAsync(cardId);
            return card.CardMembers;
        }

        public async Task<CardMember> AddMemberToCardAsync(int userId, int cardId, Role role)
        {
            User userForMembership = await _userManager.GetUserByIdAsync(userId);
            Card card = await GetForEditByIdAsync(cardId);
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (!currentCardMember.IsMemberAdmin)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Guard.Against.CheckMemebershipCard(userId, card);
            CardMember newCardMember = new CardMember(userId, role);
            card.CardMembers.Add(newCardMember);
            string actionDescription = $"Add member {userForMembership.Name}({userForMembership.Id}) with role {role}";
            CardAction cardAction = new CardAction(currentCardMember.Id, actionDescription);
            card.Actions.Add(cardAction);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            return newCardMember;
        }

        private async Task<CardMember> GetMemberByUserIdAsync(int cardId, int userId)
        {
            var memberSpec = new GetCardMemberByUserIdSpecification(userId, cardId);
            CardMember cardMember = await _cardMemberRepository.GetSingleAsync(memberSpec);
            Guard.Against.NullObject(userId, cardMember, "CardMember");
            return cardMember;
        }

        public async Task RemoveMemberFromCardAsync(int memberId, int cardId)
        {
            Card card = await GetCardWithMembersByIdAsync(cardId);
            CardMember сardMemberForRemoveFromMembership = GetCardMemberByMemberId(card, memberId);
            User userForRemoveFromMembership = await _userManager.GetUserByIdAsync(сardMemberForRemoveFromMembership.UserId);
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (!currentCardMember.IsMemberAdmin)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            card = await GetForEditByIdAsync(cardId);
            CardMember сardMember = GetCardMemberByMemberId(card, memberId);
            card.CardMembers.Remove(сardMember);
            string actionDescription = $"Remove user {userForRemoveFromMembership.Name}({userForRemoveFromMembership.Id}) from membership";
            CardAction cardAction = new CardAction(currentCardMember.Id, actionDescription);
            card.Actions.Add(cardAction);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateMembershipOfMemberOnCardAsync(int cardId, int memberId, Role newRole)
        {
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (!currentCardMember.IsMemberAdmin)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Card card = await GetForEditByIdAsync(cardId);
            CardMember cardMember = card.CardMembers.Where(c => c.Id == memberId).FirstOrDefault();
            User user = await _userManager.GetUserByIdAsync(cardMember.UserId);
            Guard.Against.NullObject(cardMember.UserId, user, "User");
            Role oldRole = cardMember.Role;
            cardMember.Role = newRole;
            string description = $"Update membership for user {user.Name}({user.Id}) from {oldRole} to {newRole}";
            card.Actions.Add(new CardAction(currentCardMember.Id, description));
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task<CardAction> AddCommentToCardAsync(int cardId, string comment)
        {
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (currentCardMember != null && !currentCardMember.CanUpdate)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Card card = await GetForEditByIdAsync(cardId);
            CardAction cardAction = new CardAction(currentCardMember.Id, comment, true);
            card.Actions.Add(cardAction);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            return cardAction;
        }

        public async Task<IEnumerable<CardAction>> GetCommentsOnCardAsync(int cardId)
        {
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (currentCardMember != null && !currentCardMember.CanRead)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Card card = await GetCardByIdAsync(cardId);
            return card.Actions.Where(a => a.IsComment);
        }
        public async Task<IEnumerable<CardAction>> GetCommentsOnCardAsync(int cardId, int commentId)
        {
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (currentCardMember != null && !currentCardMember.CanRead)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Card card = await GetCardByIdAsync(cardId);
            return card.Actions.Where(a => a.Id == commentId && a.IsComment);
        }

        public async Task DeleteCommentOnCardAsync(int cardId, int commentId)
        {
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (currentCardMember != null && !currentCardMember.CanUpdate)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Card card = await GetForEditByIdAsync(cardId);
            CardAction cardAction = card.Actions.FirstOrDefault(a => a.Id == commentId);
            Guard.Against.NullObject(commentId, cardAction, "Comment");
            if (cardAction.MemberId != currentCardMember.Id) // the user can delete his own comments
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            card.Actions.Remove(cardAction);
            string description = $"Delete Comment";
            card.Actions.Add(new CardAction(currentCardMember.Id, description));
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task MoveCardToListAsync(int cardId, int newListId)
        {
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (currentCardMember != null && !currentCardMember.CanUpdate)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Card card = await GetForEditByIdAsync(cardId);
            List newList = await GetListByIdAsync(newListId);
            List oldList = await GetListByIdAsync(card.List.Id);
            if (newList.BoardId != oldList.BoardId)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "The card can be moved to another list only on the same board.");
            }
            card.List = newList;
            string description = $"Move Card from list {oldList.Name} to list {newList}";
            card.Actions.Add(new CardAction(currentCardMember.Id, description));
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        private async Task<Card> GetForEditByIdAsync(int cardId)
        {
            Card card = await _cardRepository.GetForEditByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            return card;
        }

        private async Task<Card> GetCardByIdAsync(int cardId)
        {
            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            return card;
        }

        private async Task<Card> GetCardWithMembersByIdAsync(int cardId)
        {
            Card card = await _cardRepository.GetWithMembersAsync(cardId);
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
        private async Task<CardMember> GetCardMemberByUserId(int cardId, int userId)
        {
            var memberSpec = new GetCardMemberByUserIdSpecification(userId, cardId);
            CardMember cardMember = await _cardMemberRepository.GetSingleAsync(memberSpec);
            Guard.Against.NullObject(userId, cardMember, "CardMember");
            return cardMember;
        }
        private async Task<CardMember> GetCurrentCardMemberAsync(int cardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            return await GetCardMemberByUserId(cardId, currentUserId);
        }
    }
}
