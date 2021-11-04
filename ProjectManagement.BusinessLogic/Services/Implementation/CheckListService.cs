using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.BusinessLogic.Specifications;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class CheckListService : ICheckListService
    {
        private readonly ICheckListRepository _checkListRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IUserManager _userManager;
        public CheckListService(ICheckListRepository CheckListRepository, ICardRepository cardRepository, IUserManager userManager)
        {
            _checkListRepository = CheckListRepository;
            _cardRepository = cardRepository;
            _userManager = userManager;
        }
        public async Task<CheckList> CreateCheckListAsync(int cardId, string name)
        {
            CardMember currentCardMember = await GetCurrentCardMember(cardId);
            if (!currentCardMember.CanUpdate)
            {
                throw new Exception();
            }
            Card card = await GetForEditByIdAsync(cardId);
            CheckList checkList = new CheckList(name);
            string actionDescription = $"Add CheckList {name}";
            CardAction cardAction = new CardAction(currentCardMember.Id, actionDescription);
            card.Actions.Add(cardAction);
            card.CheckLists.Add(checkList);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            return checkList;
        }
        public async Task<CheckListItem> AddCheckListItemToCheckListAsync(int checkListId, string name)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            CheckList checkList = await GetCheckListByIdAsync(checkListId);
            CardMember currentCardMember = await GetCurrentCardMember(checkList.CardId);
            if (!currentCardMember.CanUpdate)
            {
                throw new AccessViolationException("Violation Exception while accessing the resource.");
            }
            Card card = await GetForEditByIdAsync(checkList.CardId);
            CheckListItem checkListItem = new CheckListItem(name);
            string actionDescription = $"Add CheckListItem {checkListItem.Name} to CheckList {checkList.Name}";
            CheckList checkListForEdit = card.CheckLists.Where(cl => cl.Id == checkListId).FirstOrDefault();
            Guard.Against.NullObject(checkListId, checkListForEdit, "CheckList");
            checkListForEdit.ChecklistItems.Add(checkListItem);
            CardAction cardAction = new CardAction(currentCardMember.Id, actionDescription);
            card.Actions.Add(cardAction);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            return checkListItem;
        }
        public async Task<IEnumerable<CheckList>> GetCheckListsByCardIdAsync(int cardId)
        {
            CardMember currentCardMember = await GetCurrentCardMember(cardId);
            if (!currentCardMember.CanRead)
            {
                throw new AccessViolationException("Violation Exception while accessing the resource.");
            }
            Card card = await GetCardByIdAsync(cardId);
            return card.CheckLists;
        }
        public async Task<IEnumerable<CheckListItem>> GetCheckListItemsAsync(int checkListId)
        {
            CheckList checkList = await GetCheckListByIdAsync(checkListId);
            Card card = await GetCardByIdAsync(checkList.CardId);
            CardMember currentCardMember = await GetCurrentCardMember(checkList.CardId);
            if (!currentCardMember.CanRead)
            {
                throw new AccessViolationException("Violation Exception while accessing the resource.");
            }
            return checkList.ChecklistItems;
        }
        public async Task CompleteCheckListItemAsync(int checkListId, int checkListItemId)
        {
            CheckList checkList = await GetCheckListByIdAsync(checkListId);
            CardMember currentCardMember = await GetCurrentCardMember(checkList.CardId);
            if (!currentCardMember.CanRead)
            {
                throw new AccessViolationException("Violation Exception while accessing the resource.");
            }
            Card card = await GetForEditByIdAsync(checkList.CardId);
            CheckList checkListForEdit = card.CheckLists.Where(cl => cl.Id == checkListId).FirstOrDefault();
            CheckListItem checkListItem = checkListForEdit.ChecklistItems.Where(cli => cli.Id == checkListItemId).FirstOrDefault();
            Guard.Against.NullObject(checkListItemId, checkListItem, "CheckListItem");
            checkListItem.IsDone = true;
            string actionDescription = $"Complete  CheckListItem {checkListItem.Name}({checkListItem.Id})";
            CardAction cardAction = new CardAction(currentCardMember.Id, actionDescription);
            card.Actions.Add(cardAction);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }
        public async Task DeleteCheckListAsync(int checkListId)
        {
            CheckList checkList = await GetCheckListByIdAsync(checkListId);
            CardMember currentCardMember = await GetCurrentCardMember(checkList.CardId);
            if (!currentCardMember.CanUpdate)
            {
                throw new AccessViolationException("Violation Exception while accessing the resource.");
            }
            Card card = await GetForEditByIdAsync(checkList.CardId);
            CheckList checkListForDeleting = card.CheckLists.Where(cl => cl.Id == checkListId).FirstOrDefault();
            card.CheckLists.Remove(checkListForDeleting);
            string actionDescription = $"Delete CheckList {checkList.Name}({checkList.Id})";
            CardAction cardAction = new CardAction(currentCardMember.Id, actionDescription);
            card.Actions.Add(cardAction);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCheckListItemAsync(int checkListId, int checkListItemId)
        {
            CheckList checkList = await GetCheckListByIdAsync(checkListId);
            CardMember currentCardMember = await GetCurrentCardMember(checkList.CardId);
            if (!currentCardMember.CanUpdate)
            {
                throw new AccessViolationException("Violation Exception while accessing the resource.");
            }
            Card card = await GetForEditByIdAsync(checkList.CardId);
            CheckList checkListForEdit = card.CheckLists.Where(cl => cl.Id == checkList.Id).FirstOrDefault();
            CheckListItem checkListItem = checkListForEdit.ChecklistItems.FirstOrDefault(cli => cli.Id == checkListItemId);
            Guard.Against.NullObject(checkListItemId, checkListItem, "ChecklistItem");
            checkListForEdit.ChecklistItems.Remove(checkListItem);
            string actionDescription = $"Delete CheckListItem {checkListItem.Name}({checkListItem.Id})";
            CardAction cardAction = new CardAction(currentCardMember.Id, actionDescription);
            card.Actions.Add(cardAction);
            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
        }
        private async Task<Card> GetCardByIdAsync(int cardId)
        {
            Card card = await _cardRepository.GetWithItemsByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            return card;
        }
        private async Task<CheckList> GetCheckListByIdAsync(int checkListId)
        {
            CheckList checkList = await _checkListRepository.GetWithItemsAsync(checkListId);
            Guard.Against.NullObject(checkListId, checkList, "CheckList");
            return checkList;
        }

        private async Task<CardMember> GetCurrentCardMember(int cardId)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            Card card = await GetCardByIdAsync(cardId);
            CardMember cardMember = card.CardMembers.FirstOrDefault(cm => cm.UserId == currentUserId);
            Guard.Against.NullObject(currentUserId, cardMember, "CardMember");
            return cardMember;
        }
        private async Task<Card> GetForEditByIdAsync(int cardId)
        {
            Card card = await _cardRepository.GetForEditByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            return card;
        }
    }
}
