using System.Collections.Generic;
using System.Linq;
using UnixStyleContainerFileFramework.Utils;

namespace UnixStyleContainerFileFramework.Blocks
{
        internal class FreeBlocksBlock : Block
        {
                public Stack<uint> FreeBlocks { get; set; }

                internal override void Deserialize(DataReader dataReader)
                {
                        uint size = dataReader.ReadUInt32();

                        FreeBlocks = new Stack<uint>();
                        for (int i = 0; i < size; i++)
                        {
                                FreeBlocks.Push(dataReader.ReadUInt32());
                        }
                }

                internal override void Serialize(DataWriter dataWriter)
                {
                        dataWriter.WriteUInt32((uint) FreeBlocks.Count);

                        foreach (uint freeBlock in FreeBlocks.Reverse())
                        {
                                dataWriter.WriteUInt32(freeBlock);
                        }
                }
        }
}