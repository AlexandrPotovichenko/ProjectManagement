using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class CardRepository : BaseRepository<Card, int, ProjectManagementContext>, ICardRepository
    {
        public CardRepository(ProjectManagementContext context) : base(context)
        {
        }
    }
}
