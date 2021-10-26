using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.DataAccess.Repositories.Interfaces
{
    public interface ICheckListRepository : IRepository<CheckList, int>
    {
        Task<CheckList> GetWithItemsAsync(int checkListId);
        Task<CheckList> GetCheckListByCheckListItemId(int checkListItemId);
    }
}
