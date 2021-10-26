using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface ICheckListService
    {
        Task<CheckList> CreateCheckListAsync(int cardId, string name);
        Task<CheckListItem> AddCheckListItemToCheckListAsync(int checkListId, string name);

        Task<IEnumerable<CheckListItem>> GetCheckListItemsAsync(int checkListId);
        Task<IEnumerable<CheckList>> GetCheckListsByCardIdAsync(int cardId);

        Task CompleteCheckListItemAsync(int checkListItemId);

        Task DeleteCheckListItemAsync(int checkListItemId);
        Task DeleteCheckListAsync(int checkListId);
        
    }
}
