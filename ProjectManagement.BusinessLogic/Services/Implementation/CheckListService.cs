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

            int currentUserId = _userManager.GetCurrentUserId();

            Card card = await GetCardByIdAsync(cardId);

            CardMember cardMember = card.CardMembers.Where(cm => cm.UserId == currentUserId).FirstOrDefault();
            if (!cardMember.CanUpdate)
            {
                throw new Exception();
            }


            

            CheckList checkList = new CheckList(name);

            string actionDescription = $"Add CheckList {name}";
            CardAction cardAction = new CardAction(cardMember, actionDescription);
            card.Actions.Add(cardAction);

            card.CheckLists.Add(checkList);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();

            return checkList;
        }
        public async Task<CheckListItem> AddCheckListItemToListAsync(int checkListId, string name)
        {
            int currentUserId = _userManager.GetCurrentUserId();
            CheckList checkList = await GetCheckListByIdAsync(checkListId);
            Card card = await GetCardByIdAsync(checkList.CardId);


            CardMember currentCardMember = GetCardMemberByUserId(card, currentUserId);



            //CardMember currentCardMember = card.CardMembers.Where(cm => cm.UserId == currentUserId).FirstOrDefault();
            if (!currentCardMember.CanUpdate)
            {
                throw new Exception();
            }

            

            var item = await _checkListRepository.GetByIdAsync(checkListId);

            CheckListItem checkListItem = new CheckListItem(name);
            item.ChecklistItems.Add(checkListItem);



            string actionDescription = $"Add CheckListItem {checkListItem.Name} to CheckList {checkList.Name}";

            card.CheckLists.Add(item);

            CardAction cardAction = new CardAction(currentCardMember, actionDescription);
            card.Actions.Add(cardAction);



            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();


            //await _checkListRepository.UpdateAsync(item);
            //await _checkListRepository.UnitOfWork.SaveChangesAsync();

            return checkListItem;
        }
        public async Task<IEnumerable<CheckList>> GetCheckListsAsync(int cardId)
        {
            Card card = await GetCardByIdAsync(cardId);
            return card.CheckLists;
        }
        public async Task<IEnumerable<CheckListItem>> GetCheckListItemsAsync(int checkListId)
        {
            CheckList checkList = await GetCheckListByIdAsync(checkListId);
            return checkList.ChecklistItems;
        }
        public async Task CompleteCheckListItemAsync(int checkListId, int checkListItemId)
        {
            
            CheckList checkList = await GetCheckListByIdAsync(checkListId);
            int currentUserId = _userManager.GetCurrentUserId();
            Card card = await GetCardByIdAsync(checkList.CardId);
            CardMember currentCardMember = GetCardMemberByUserId(card, currentUserId);
            if (!currentCardMember.CanUpdate)
            {
                throw new Exception();
            }

            CheckListItem checkListItem = checkList.ChecklistItems.FirstOrDefault();
            Guard.Against.NullObject(checkListItemId, checkListItem, "CheckListItem");
            checkListItem.IsDone = true;



            string actionDescription = $"Complete  CheckListItem {checkListItem.Name}({checkListItem.Id})";
            CardAction cardAction = new CardAction(currentCardMember, actionDescription);
            card.Actions.Add(cardAction);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();
            //string description = $"{user.Name}({user.Id}) Complete CheckListItem {checkListItem.Name}({user.Id}) from {oldRole} to {newRole}";

            //int currentUserId = _userManager.GetCurrentUserId();
            //Card card = await GetCardByIdAsync(checkList.CardId);
            //string description = $"Delete Comment";
            //card.Actions.Add(new CardAction(currentCardMember, description));

            await _checkListRepository.UpdateAsync(checkList);
            await _checkListRepository.UnitOfWork.SaveChangesAsync();
        }
        public async Task DeleteCheckListAsync(int checkListId)
        {
            CheckList checkList = await GetCheckListByIdAsync(checkListId);

            int currentUserId = _userManager.GetCurrentUserId();
            Card card = await GetCardByIdAsync(checkList.CardId);
            CardMember currentCardMember = GetCardMemberByUserId(card, currentUserId);
            if (!currentCardMember.CanUpdate)
            {
                throw new Exception();
            }

            await _checkListRepository.DeleteAsync(checkList);

            string actionDescription = $"Delete CheckList {checkList.Name}({checkList.Id})";
            CardAction cardAction = new CardAction(currentCardMember, actionDescription);
            card.Actions.Add(cardAction);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();

        }

        public async Task DeleteCheckListItemAsync(int checkListId, int checkListItemId)
        {
            CheckList checkList = await GetCheckListByIdAsync(checkListId);


            int currentUserId = _userManager.GetCurrentUserId();
            Card card = await GetCardByIdAsync(checkList.CardId);
            CardMember currentCardMember = GetCardMemberByUserId(card, currentUserId);
            if (!currentCardMember.CanUpdate)
            {
                throw new Exception();
            }


            CheckListItem checkListItem = checkList.ChecklistItems.FirstOrDefault(cli => cli.Id == checkListItemId);
            Guard.Against.NullObject(checkListItemId, checkListItem, "ChecklistItem");

            checkList.ChecklistItems.Remove(checkListItem);

 await _checkListRepository.UpdateAsync(checkList);

            string actionDescription = $"Delete CheckListItem {checkListItem.Name}({checkListItem.Id})";
            CardAction cardAction = new CardAction(currentCardMember, actionDescription);
            card.Actions.Add(cardAction);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();

        }
        private async Task<Card> GetCardByIdAsync(int cardId)
        {
            Card card = await _cardRepository.GetWithMembersAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");
            return card;
        }
        private async Task<CheckList> GetCheckListByIdAsync(int checkListId)
        {
            CheckList checkList = await _checkListRepository.GetWithItemsAsync(checkListId);
            Guard.Against.NullObject(checkListId, checkList, "CheckList");
            return checkList;
        }
        private CardMember GetCardMemberByUserId(Card card, int userId)
        {
            CardMember cardMember = card.CardMembers.FirstOrDefault(cm => cm.UserId == userId);
            Guard.Against.NullObject(userId, cardMember, "CardMember");
            return cardMember;
        }
    }
}
