using Ardalis.Specification;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Specifications
{
    internal class GetCommentsOnCardSpecification : Specification<CardAction>
    {
        private int cardId;

        public GetCommentsOnCardSpecification(int cardId)
        {
            //Query.Where(c => c.IsComment.ChecklistItems..Any(cli => cli.checklistItemId));
            //AddInclude(x => x.Group);
        }

        //public GetBoardsSpecification(int cardId)
        //{
        //    this.cardId = cardId;
        //}
    }
}