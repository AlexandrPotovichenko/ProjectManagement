using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class CardMemberRepository : BaseRepository<CardMember, int, ProjectManagementContext>, ICardMemberRepository
    {
        public CardMemberRepository(ProjectManagementContext context) : base(context)
        {
        }
    }
}
