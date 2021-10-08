using System;

namespace ProjectManagement.BusinessLogic.Exceptions
{
    public class MemberAlreadyExistsException : Exception
    {
        public MemberAlreadyExistsException(string message, int memberId) : base(message)
        {
            DuplicateItemId = memberId;
        }

        public int DuplicateItemId { get; }
    }
}
