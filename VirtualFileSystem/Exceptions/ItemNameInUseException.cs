using System;
using System.Runtime.Serialization;

namespace VirtualFileSystem.Exceptions
{
        [Serializable]
        public class ItemNameInUseException : Exception
        {
                public ItemNameInUseException()
                {
                }

                public ItemNameInUseException(string message) : base(message)
                {
                }

                public ItemNameInUseException(string message, Exception innerException)
                        : base(message, innerException)
                {
                }

                protected ItemNameInUseException(SerializationInfo info, StreamingContext context)
                        : base(info, context)
                {
                }
        }
}