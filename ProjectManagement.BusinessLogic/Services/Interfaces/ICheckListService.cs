using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface ICheckListService
    {
        Task<IEnumerable<CheckListItem>> GetCheckListItemsAsync(int checkListId);
        //Task<IEnumerable<CheckListItem>> GetCompletedCheckListItemsAsync(int userId);
        //Task CompleteCheckListItemAsync(int userId, int CheckListItemId);
        Task<CheckList> CreateCheckListAsync(int cardId, string name);
        Task<CheckListItem> AddCheckListItemAsync(int checkListId, string name);
    }
}
