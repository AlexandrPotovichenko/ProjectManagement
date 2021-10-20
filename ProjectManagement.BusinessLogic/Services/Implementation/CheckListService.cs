using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
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

        public CheckListService(ICheckListRepository CheckListRepository, ICardRepository cardRepository)
        {
            _checkListRepository = CheckListRepository;
            _cardRepository = cardRepository;
        }

        public async Task<IEnumerable<CheckListItem>> GetCheckListItemsAsync(int checkListId)
        {
            var specification = new GetCheckListItemsOfCheckListSpecification(checkListId);
            CheckList checkList = await _checkListRepository.GetByIdAsync(checkListId);
            return checkList.ChecklistItems;
        }

        public async Task<CheckListItem> AddCheckListItemAsync(int checkListId,string name)
        {

            CheckList checkList = await _checkListRepository.GetByIdAsync(checkListId);
            Guard.Against.NullObject(checkListId, checkList, "CheckList");

            var item = await _checkListRepository.GetByIdAsync(checkListId);
          
            CheckListItem checkListItem = new CheckListItem(name);
            item.ChecklistItems.Add(checkListItem);

            await _checkListRepository.UpdateAsync(item);
            await _checkListRepository.UnitOfWork.SaveChangesAsync();

            return checkListItem;
        }

        public async Task<CheckList> CreateCheckListAsync(int cardId, string name)
        {

            Card card = await _cardRepository.GetByIdAsync(cardId);
            Guard.Against.NullObject(cardId, card, "Card");

            CheckList checkList = new CheckList(name);

            card.CheckLists.Add(checkList);

            await _cardRepository.UpdateAsync(card);
            await _cardRepository.UnitOfWork.SaveChangesAsync();

            return checkList;

        }

        public async Task DeleteCheckListItemAsync(int checkListId, int checkListItemId)
        {
            CheckList checkList = await _checkListRepository.GetByIdAsync(checkListId);
            Guard.Against.NullObject(checkListId, checkList, "CheckList");

            CheckListItem checkListItem = checkList.ChecklistItems.FirstOrDefault(cli => cli.Id == checkListItemId);
            Guard.Against.NullObject(checkListItemId, checkListItem, "ChecklistItem");

            checkList.ChecklistItems.Remove(checkListItem);

            await _checkListRepository.UpdateAsync(checkList);
            await _checkListRepository.UnitOfWork.SaveChangesAsync();

        }

        public async Task DeleteCheckListAsync(int checkListId)
        {
            CheckList checkList = await _checkListRepository.GetByIdAsync(checkListId);
            Guard.Against.NullObject(checkListId, checkList, "CheckList");
            await _checkListRepository.DeleteAsync(checkList);
        }

 
    }
}
