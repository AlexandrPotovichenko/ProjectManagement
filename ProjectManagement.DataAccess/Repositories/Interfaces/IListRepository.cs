using ProjectManagement.Domain.Models;
using System.Threading.Tasks;

namespace ProjectManagement.DataAccess.Repositories.Interfaces
{
    public interface IListRepository : IRepository<List, int>
    {
        Task<List> GetForEditByIdAsync(int listId);
    }
}
