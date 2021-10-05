using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.BusinessLogic.Services.Interfaces
{
    public interface IBoardService
    {
        Task<IEnumerable<Board>> GetBoardsAsync();
        Task<Board> CreateBoardAsync(string name, 
            string description, int userId);
    }
}