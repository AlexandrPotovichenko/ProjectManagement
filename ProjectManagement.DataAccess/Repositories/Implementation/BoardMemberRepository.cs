using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class BoardMemberRepository : BaseRepository<BoardMember, int, ProjectManagementContext>, IBoardMemberRepository
    {
        public BoardMemberRepository(ProjectManagementContext context) : base(context)
        {
        }
    }
}
