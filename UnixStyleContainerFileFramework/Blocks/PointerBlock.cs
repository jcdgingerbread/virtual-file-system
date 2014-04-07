using System.Diagnostics.CodeAnalysis;
using UnixStyleContainerFileFramework.Utils;

namespace UnixStyleContainerFileFramework.Blocks
{
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class PointerBlock : Block
        {
                public const uint POINTERS = SIZE / 4;

                public uint[] DataBlocks { get; private set; }

                internal override void Deserialize(DataReader dataReader)
                {
                        DataBlocks = new uint[POINTERS];

                        for (int i = 0; i < POINTERS; i++)
                        {
                                DataBlocks[i] = dataReader.ReadUInt32();
                        }
                }

                internal override void Serialize(DataWriter dataWriter)
                {
                        foreach (uint dataBlock in DataBlocks)
                        {
                                dataWriter.WriteUInt32(dataBlock);
                        }
                }
        }
}