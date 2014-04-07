using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnixStyleContainerFileFramework.Exceptions;
using UnixStyleContainerFileFramework.Structural;
using UnixStyleContainerFileFramework.Utils;

namespace UnixStyleContainerFileFramework.Blocks
{
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        internal class SuperBlock : Block
        {
                public const uint SUPER_BLOCK_LOCATION = 0;

                public const uint STATIC_SIZE = 10 * sizeof (uint);

                public uint TotalBlocks { internal get; set; }
                public uint FreeBlocks { get; set; }

                public uint HighestDataBlock { get; set; }


                public uint TotalInodes { get; set; }
                public uint FreeIndexNodes { get; set; }

                public uint IndexNodesBlocks { private get; set; }


                public uint FreeIndexNodesQueueMaxSize { private get; set; }
                // must be != null
                public Queue<uint> FreeIndexNodesQueue { private get; set; }

                public uint FreeBlockStackMaxSize { private get; set; }
                // must be != null
                public Stack<uint> FreeBlockStack { private get; set; }

                internal override void Deserialize(DataReader dataReader)
                {
                        TotalBlocks = dataReader.ReadUInt32();
                        FreeBlocks = dataReader.ReadUInt32();
                        HighestDataBlock = dataReader.ReadUInt32();
                        TotalInodes = dataReader.ReadUInt32();
                        FreeIndexNodes = dataReader.ReadUInt32();
                        IndexNodesBlocks = dataReader.ReadUInt32();
                        FreeIndexNodesQueueMaxSize = dataReader.ReadUInt32();

                        FreeIndexNodesQueue = new Queue<uint>();
                        uint freeIndexNodesQueueSize = dataReader.ReadUInt32();
                        for (int i = 0; i < freeIndexNodesQueueSize; i++)
                        {
                                FreeIndexNodesQueue.Enqueue(dataReader.ReadUInt32());
                        }


                        FreeBlockStackMaxSize = dataReader.ReadUInt32();

                        FreeBlockStack = new Stack<uint>();
                        uint freeBlockStackSize = dataReader.ReadUInt32();
                        for (int i = 0; i < freeBlockStackSize; i++)
                        {
                                FreeBlockStack.Push(dataReader.ReadUInt32());
                        }
                }

                internal override void Serialize(DataWriter dataWriter)
                {
                        dataWriter.WriteUInt32(TotalBlocks);
                        dataWriter.WriteUInt32(FreeBlocks);
                        dataWriter.WriteUInt32(HighestDataBlock);
                        dataWriter.WriteUInt32(TotalInodes);
                        dataWriter.WriteUInt32(FreeIndexNodes);
                        dataWriter.WriteUInt32(IndexNodesBlocks);
                        dataWriter.WriteUInt32(FreeIndexNodesQueueMaxSize);

                        dataWriter.WriteUInt32((uint) FreeIndexNodesQueue.Count);
                        foreach (uint freeIndexNode in FreeIndexNodesQueue)
                        {
                                dataWriter.WriteUInt32(freeIndexNode);
                        }


                        dataWriter.WriteUInt32(FreeBlockStackMaxSize);

                        dataWriter.WriteUInt32((uint) FreeBlockStack.Count);
                        foreach (uint freeBlock in FreeBlockStack.Reverse())
                        {
                                dataWriter.WriteUInt32(freeBlock);
                        }
                }

                public uint GetFreeIndexNode()
                {
                        if (FreeIndexNodes < 1)
                        {
                                throw new OutOfFreeFilesException("out of free file numbers");
                        }

                        if (FreeIndexNodesQueue.Count == 0)
                        {
                                FreeIndexNodesQueue = CreateNewFreeQueue();
                        }

                        FreeIndexNodes--;

                        return FreeIndexNodesQueue.Dequeue();
                }

                private Queue<uint> CreateNewFreeQueue()
                {
                        // scan for free index nodes

                        var freeQueue = new Queue<uint>();

                        for (uint i = 0; i < IndexNodesBlocks; i++)
                        {
                                var indexNodesBlock = BlockManager.Request<IndexNodesBlock>(IndexNodesBlock.CATEGORY_FIRST_BLOCK_INDEX + i);
                                List<uint> freeList = indexNodesBlock.GetFreeList();
                                BlockManager.DoneUsing(indexNodesBlock, false);

                                var remainingSpace = (int) (FreeIndexNodesQueueMaxSize - freeQueue.Count);
                                IEnumerable<uint> elementsToAdd = remainingSpace > freeList.Count ? freeList : freeList.Take(remainingSpace);

                                foreach (uint element in elementsToAdd)
                                {
                                        freeQueue.Enqueue(element + i * IndexNode.INDEX_NODES_PER_BLOCK);
                                }

                                if (freeQueue.Count >= FreeIndexNodesQueueMaxSize)
                                {
                                        break;
                                }
                        }

                        return freeQueue;
                }

                public void FreeIndexNode(uint fileNumber)
                {
                        if (FreeIndexNodesQueue.Count < FreeIndexNodesQueueMaxSize)
                        {
                                FreeIndexNodesQueue.Enqueue(fileNumber);
                        }

                        FreeIndexNodes++;
                }

                public uint GetFreeDataBlock()
                {
                        if (FreeBlockStack.Count < 1)
                        {
                                throw new InvalidDataException();
                        }

                        uint freeDataBlock;

                        if (FreeBlockStack.Count == 1)
                        {
                                if (FreeBlockStack.Peek() == 0)
                                {
                                        freeDataBlock = CreateNewBlock();
                                }
                                else
                                {
                                        freeDataBlock = FreeBlockStack.Pop();

                                        var freeBlocksBlock = BlockManager.Request<FreeBlocksBlock>(freeDataBlock);

                                        FreeBlockStack = freeBlocksBlock.FreeBlocks;

                                        BlockManager.Remove(freeBlocksBlock.BlockIndex);
                                }
                        }
                        else
                        {
                                freeDataBlock = FreeBlockStack.Pop();
                        }

                        return freeDataBlock;
                }

                private uint CreateNewBlock()
                {
                        if (HighestDataBlock >= TotalBlocks)
                        {
                                throw new OutOfFreeSpaceException();
                        }

                        return ++HighestDataBlock;
                }

                public void FreeDataBlock(uint block)
                {
                        BlockManager.Remove(block);

                        if (FreeBlockStack.Count < FreeBlockStackMaxSize)
                        {
                                FreeBlockStack.Push(block);
                        }
                        else
                        {
                                var freeBlocksBlock = new FreeBlocksBlock
                                        {
                                                BlockIndex = block,
                                                FreeBlocks = FreeBlockStack
                                        };

                                BlockManager.WriteBlock(freeBlocksBlock);

                                FreeBlockStack = new Stack<uint>();

                                FreeBlockStack.Push(block);
                        }
                }
        }
}