using System;
using System.Runtime.Serialization;

namespace VirtualFileSystem.Exceptions
{
        [Serializable]
        public class RootLocationException : Exception
        {
                public RootLocationException()
                {
                }

                public RootLocationException(string message) : base(message)
                {
                }

                public RootLocationException(string message, Exception innerException) : base(message, innerException)
                {
                }

                protected RootLocationException(SerializationInfo info, StreamingContext context)
                        : base(info, context)
                {
                }
        }
}