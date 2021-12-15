﻿using Ardalis.GuardClauses;
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
            Guard.Against.NullObject(listId, list, "List");
            bool userCanCreateCard = await _cardRepository.CanCreateCardAsync(list.BoardId, currentUserId);
            if (!userCanCreateCard)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            CardMember cardMember = new CardMember(currentUserId, Role.Admin);

            Card card = new Card(name, description, listId);
            Card insertedCard = await _cardRepository.InsertAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            cardMember.Card = insertedCard;
            cardMember.CardId = insertedCard.Id;

            CardMember insertedCardMember = await _cardMemberRepository.InsertAsync(cardMember);
            await _cardRepository.UnitOfWork.SaveChangesAsync();

            insertedCard.CardMembers.Add(insertedCardMember);
            insertedCard.List = list;

            string actionDescription = $"Create Card";
            insertedCard.Actions.Add(new CardAction(insertedCardMember.Id, actionDescription));
            await _cardRepository.UpdateAsync(insertedCard);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            return insertedCard;
        }

        public async Task<Card> GetCardAsync(int cardId)
        {
            CardMember cardMember = await GetCurrentCardMemberAsync(cardId);
            if (!cardMember.CanRead)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
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

        public async Task<IEnumerable<CardMember>> GetAllCardMembersAsync(int cardId)
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
            Guard.Against.NullObject(userId, userForMembership, "User");
            Card card = await GetForEditByIdAsync(cardId);
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (!currentCardMember.IsMemberAdmin)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Guard.Against.CheckMemebershipCard(userId, card);
            CardMember newCardMember = new CardMember(userId, role);
            card.CardMembers.Add(newCardMember);
            string actionDescription = $"Add member {userForMembership.Name}({userForMembership.Id}) with role '{role}'";
            CardAction cardAction = new CardAction(currentCardMember.Id, actionDescription);
            card.Actions.Add(cardAction);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            return newCardMember;
        }

        public async Task RemoveMemberFromCardAsync(int memberId, int cardId)
        {
            Card card = await GetCardWithMembersByIdAsync(cardId);
            CardMember сardMemberForRemoveFromMembership = GetCardMemberByMemberId(card, memberId);
            User userForRemoveFromMembership = await _userManager.GetUserByIdAsync(сardMemberForRemoveFromMembership.UserId);
            Guard.Against.NullObject(сardMemberForRemoveFromMembership.UserId, userForRemoveFromMembership, "User");
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
            Guard.Against.NullObject(memberId, cardMember, "CardMember");
            User user = await _userManager.GetUserByIdAsync(cardMember.UserId);
            Guard.Against.NullObject(cardMember.UserId, user, "User");
            Role oldRole = cardMember.Role;
            cardMember.Role = newRole;
            string description = $"Update membership for user {user.Name}({user.Id}) from '{oldRole}' to '{newRole}'";
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
            Card card = await GetCardWithItemsByIdAsync(cardId);
            return card.Actions.Where(a => a.IsComment);
        }
        public async Task<IEnumerable<CardAction>> GetCommentsOnCardAsync(int cardId, int commentId)
        {
            CardMember currentCardMember = await GetCurrentCardMemberAsync(cardId);
            if (currentCardMember != null && !currentCardMember.CanRead)
            {
                throw new WebAppException((int)HttpStatusCode.NotAcceptable, "Violation Exception while accessing the resource.");
            }
            Card card = await GetCardWithItemsByIdAsync(cardId);
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
            if (cardAction.CardMemberId != currentCardMember.Id) // the user can delete his own comments
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
            string description = $"Move Card from list '{oldList.Name}' to list '{newList}'";
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
        private async Task<Card> GetCardWithItemsByIdAsync(int cardId)
        {
            Card card = await _cardRepository.GetWithItemsByIdAsync(cardId);
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
            GetCardMemberByUserIdSpecification memberSpec = new GetCardMemberByUserIdSpecification(userId, cardId);
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
