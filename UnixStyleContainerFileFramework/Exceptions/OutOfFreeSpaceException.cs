using System;
using System.Runtime.Serialization;

namespace UnixStyleContainerFileFramework.Exceptions
{
        [Serializable]
        public class OutOfFreeSpaceException : Exception
        {
                public OutOfFreeSpaceException()
                {
                }

                public OutOfFreeSpaceException(string message) : base(message)
                {
                }

                public OutOfFreeSpaceException(string message, Exception innerException) : base(message, innerException)
                {
                }

                protected OutOfFreeSpaceException(SerializationInfo info, StreamingContext context)
                        : base(info, context)
                {
                }
        }
}