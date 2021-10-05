using Ardalis.Specification;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Specifications
{
    internal class GetUserSpecification :Specification<User>
    {
        private int userId;

        //public GetUserSpecification()
        //{

        //    AddInclude(x => x.Group);
        //}

        public GetUserSpecification(int userId)
        {
            this.userId = userId;
        }
    }
}