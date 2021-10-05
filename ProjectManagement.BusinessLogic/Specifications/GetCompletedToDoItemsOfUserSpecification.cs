using Ardalis.Specification;
using System.Linq;
using ProjectManagement.Domain.Models;

namespace CheckListApplication.BusinessLogic.Specifications
{
    public class GetCompletedCheckListItemsOfUserSpecification : Specification<CheckListItem>
    {
        public GetCompletedCheckListItemsOfUserSpecification(int userId)
        {
            Query.Where(x => x.Id == userId && x.IsDone);
        }
    }
}
