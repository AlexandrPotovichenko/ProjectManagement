using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.BusinessLogic.Specifications;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Services.Implementation
{
    public class CheckListService : ICheckListService
    {
        private readonly ICheckListRepository _CheckListRepository;
        private readonly ICardRepository _CardRepository;

        public CheckListService(ICheckListRepository CheckListRepository)
        {
            _CheckListRepository = CheckListRepository;
        }

        //public async Task<IEnumerable<CheckListItem>> GetCompletedCheckListItemsAsync(int userId)
        //{
        //    var specification = new GetCompletedCheckListItemsOfUserSpecification(userId);
        //    return await _CheckListItemRepository.GetManyAsync(specification);
        //}

        public async Task<IEnumerable<CheckListItem>> GetCheckListItemsAsync(int checkListId)
        {
            var specification = new GetCheckListItemsOfCheckListSpecification(checkListId);
            CheckList checkList = await _CheckListRepository.GetByIdAsync(checkListId);
            return checkList.ChecklistItems;
        }

        //public async Task CompleteCheckListItemAsync(int userId, int CheckListItemId)
        //{
        //    var item = await _CheckListItemRepository.GetByIdAsync(CheckListItemId);

        //    if (item is null)
        //    {
        //        throw new System.Exception();
        //    }

        //    if (item.UserId != userId)
        //    {
        //        throw new System.Exception();
        //    }

        //    item.IsCompleted = true;
        //    await _CheckListItemRepository.UpdateAsync(item);
        //    await _CheckListItemRepository.UnitOfWork.SaveChangesAsync();
        //}

        public async Task<CheckListItem> AddCheckListItemAsync(int checkListId,string name)
        {

            var item = await _CheckListRepository.GetByIdAsync(checkListId);

            if (item is null)
            {
                throw new System.Exception();// гозирага
            }
            CheckListItem checkListItem = new CheckListItem(name);
            item.ChecklistItems.Add(checkListItem);
            //    if (item.UserId != userId)
            //    {
            //        throw new System.Exception();
            //    }

            //    item.IsCompleted = true;
            //    await _CheckListItemRepository.UpdateAsync(item);
            //    await _CheckListItemRepository.UnitOfWork.SaveChangesAsync();
     

            await _CheckListRepository.UpdateAsync(item);
            await _CheckListRepository.UnitOfWork.SaveChangesAsync();

            return checkListItem;
        }

        public async Task<CheckList> CreateCheckListAsync(int cardId, string name)
        {

            var card = await _CardRepository.GetByIdAsync(cardId);
            if (card is null)
            {
                throw new System.Exception();// гозирага
            }
            CheckList checkList = new CheckList(name);

            card.CheckLists.Add(checkList);

            await _CardRepository.UpdateAsync(card);

            await _CardRepository.UnitOfWork.SaveChangesAsync();

            return checkList;

        }
    }
}
