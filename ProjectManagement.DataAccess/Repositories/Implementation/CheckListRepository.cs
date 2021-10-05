using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.DataAccess.Repositories.Implementation
{
    public class CheckListRepository : BaseRepository<CheckList, int, ProjectManagementContext>, ICheckListRepository
    {
        public CheckListRepository(ProjectManagementContext context) : base(context)
        {
        }
    }
}
