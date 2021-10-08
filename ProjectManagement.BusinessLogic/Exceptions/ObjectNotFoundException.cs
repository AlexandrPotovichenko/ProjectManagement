using System;

namespace ProjectManagement.BusinessLogic.Exceptions
{
    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException(int objectId, string objName) : base($"No {objName} found with id {objectId}")
        {
        }

        protected ObjectNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public ObjectNotFoundException(string message) : base(message)
        {
        }

        public ObjectNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
