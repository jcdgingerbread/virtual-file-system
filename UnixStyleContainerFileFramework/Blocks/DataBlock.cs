using System.Diagnostics.CodeAnalysis;
using UnixStyleContainerFileFramework.Utils;

namespace UnixStyleContainerFileFramework.Blocks
{
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class DataBlock : Block
        {
                public byte[] Data { get; private set; }

                internal override void Deserialize(DataReader dataReader)
                {
                        Data = dataReader.Read(SIZE);
                }

                internal override void Serialize(DataWriter dataWriter)
                {
                        dataWriter.WriteBytes(Data);
                }
        }
}