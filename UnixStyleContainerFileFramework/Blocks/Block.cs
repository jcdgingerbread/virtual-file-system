using System;
using UnixStyleContainerFileFramework.IOCaching;
using UnixStyleContainerFileFramework.Utils;

namespace UnixStyleContainerFileFramework.Blocks
{
        internal abstract class Block
        {
                public const int SIZE = 512 / 8;

                public uint BlockIndex { get; set; }
                public BlockManager BlockManager { protected get; set; }

                internal abstract void Deserialize(DataReader dataReader);

                internal abstract void Serialize(DataWriter dataWriter);

                public static uint BlocksNeeded(long sizeInBytes)
                {
                        try
                        {
                                return checked((uint) DivideAndCeil(sizeInBytes, SIZE));
                        }
                        catch (OverflowException)
                        {
                                throw new InsufficientMemoryException();
                        }
                }

                private static long DivideAndCeil(long a, uint b)
                {
                        if (a % b > 0)
                        {
                                return a / b + 1;
                        }

                        return a / b;
                }
        }
}