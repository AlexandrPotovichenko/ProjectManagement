using Ardalis.Specification;
using System.Linq;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Specifications
{
    public class GetCheckListItemsOfCheckListSpecification : Specification<CheckListItem>
    {
        public GetCheckListItemsOfCheckListSpecification(int checkListId)
        {
            Query.Where(x => x.CheckListId == checkListId);
        }
    }
}
