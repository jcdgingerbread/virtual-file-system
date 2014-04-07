using System;
using System.Runtime.Serialization;

namespace UnixStyleContainerFileFramework.Exceptions
{
        [Serializable]
        public class OutOfFreeFilesException : Exception
        {
                public OutOfFreeFilesException()
                {
                }

                public OutOfFreeFilesException(string message) : base(message)
                {
                }

                public OutOfFreeFilesException(string message, Exception innerException) : base(message, innerException)
                {
                }

                protected OutOfFreeFilesException(SerializationInfo info, StreamingContext context)
                        : base(info, context)
                {
                }
        }
}