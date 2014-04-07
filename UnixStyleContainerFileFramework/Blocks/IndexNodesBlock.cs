using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnixStyleContainerFileFramework.Structural;
using UnixStyleContainerFileFramework.Utils;

namespace UnixStyleContainerFileFramework.Blocks
{
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class IndexNodesBlock : Block
        {
                public const uint CATEGORY_FIRST_BLOCK_INDEX = 1;

                public IndexNode[] IndexNodes { get; private set; }

                internal override void Deserialize(DataReader dataReader)
                {
                        IndexNodes = new IndexNode[IndexNode.INDEX_NODES_PER_BLOCK];

                        for (int i = 0; i < IndexNodes.Length; i++)
                        {
                                IndexNodes[i] = IndexNode.Deserialize(dataReader, BlockManager);
                        }
                }

                internal override void Serialize(DataWriter dataWriter)
                {
                        if (IndexNodes == null) return;

                        foreach (IndexNode indexNode in IndexNodes)
                        {
                                indexNode.Serialize(dataWriter);
                        }
                }

                public List<uint> GetFreeList()
                {
                        var result = new List<uint>();

                        if (IndexNodes != null)
                        {
                                for (uint i = 0; i < IndexNodes.Length; i++)
                                {
                                        if (!InUse(IndexNodes[i]))
                                        {
                                                result.Add(i);
                                        }
                                }
                        }

                        return result;
                }

                private static bool InUse(IndexNode indexNode)
                {
                        return indexNode.DataBlocks > 0;
                }
        }
}