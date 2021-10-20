using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface ICheckListService
    {
        Task<CheckList> CreateCheckListAsync(int cardId, string name);
        Task<IEnumerable<CheckListItem>> GetCheckListItemsAsync(int checkListId);
        Task<CheckListItem> AddCheckListItemAsync(int checkListId, string name);
        Task DeleteCheckListItemAsync(int checkListId, int checkListItemId);
        Task DeleteCheckListAsync(int checkListId);
        
    }
}
