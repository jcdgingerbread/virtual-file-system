using System;
using System.Runtime.Serialization;

namespace VirtualFileSystem.Exceptions
{
        [Serializable]
        public class RootDeletionException : Exception
        {
                public RootDeletionException()
                {
                }

                public RootDeletionException(string message) : base(message)
                {
                }

                public RootDeletionException(string message, Exception innerException) : base(message, innerException)
                {
                }

                protected RootDeletionException(SerializationInfo info, StreamingContext context)
                        : base(info, context)
                {
                }
        }
}