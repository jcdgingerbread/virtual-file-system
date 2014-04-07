using System;
using System.Runtime.Serialization;

namespace UnixStyleContainerFileFramework.Exceptions
{
        [Serializable]
        public class InvalidFileCountException : Exception
        {
                public InvalidFileCountException()
                {
                }

                public InvalidFileCountException(string message) : base(message)
                {
                }

                public InvalidFileCountException(string message, Exception innerException) : base(message, innerException)
                {
                }

                protected InvalidFileCountException(SerializationInfo info, StreamingContext context)
                        : base(info, context)
                {
                }
        }
}