using Ardalis.Specification;
using ProjectManagement.Domain.Models;

namespace ProjectManagement.BusinessLogic.Specifications
{
    internal class GetUserSpecification :Specification<User>
    {
        private int userId;

        public GetUserSpecification(int userId)
        {
            this.userId = userId;
        }
    }
}