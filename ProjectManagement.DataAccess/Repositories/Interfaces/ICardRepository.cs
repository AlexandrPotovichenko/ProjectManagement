using ProjectManagement.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.DataAccess.Repositories.Interfaces
{
    public interface ICardRepository: IRepository<Card, int>
    {
        Task<IEnumerable<Card>> GetWithItemsAsync(int userId);
        Task<Card> GetWithMembersAsync(int cardId);
        Task<Card> GetWithItemsByIdAsync(int cardId);
        Task<Card> GetForEditByIdAsync(int cardId); 
       Task<bool> CanCreateCardAsync(int boardId, int userId);
    }
}
