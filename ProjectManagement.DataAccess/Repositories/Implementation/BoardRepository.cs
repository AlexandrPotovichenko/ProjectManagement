using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class BoardRepository : BaseRepository<Board, int, ProjectManagementContext>, IBoardRepository
    {
        public BoardRepository(ProjectManagementContext context) : base(context)
        {
        }
    }
}
