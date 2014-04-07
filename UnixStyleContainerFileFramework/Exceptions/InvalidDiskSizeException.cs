using System;
using System.Runtime.Serialization;

namespace UnixStyleContainerFileFramework.Exceptions
{
        [Serializable]
        public class InvalidDiskSizeException : Exception
        {
                public InvalidDiskSizeException()
                {
                }

                public InvalidDiskSizeException(string message) : base(message)
                {
                }

                public InvalidDiskSizeException(string message, Exception innerException) : base(message, innerException)
                {
                }

                protected InvalidDiskSizeException(SerializationInfo info, StreamingContext context) : base(info, context)
                {
                }
        }
}