using System;
using System.Collections.Generic;
using System.IO;
using UnixStyleContainerFileFramework.Blocks;
using UnixStyleContainerFileFramework.Exceptions;
using UnixStyleContainerFileFramework.IOCaching;
using UnixStyleContainerFileFramework.Structural;

namespace UnixStyleContainerFileFramework
{
        public class ContainerFile
        {
                private readonly BlockManager _blockManager;
                private readonly string _filePath;

                private ContainerFile(string filePath)
                {
                        _filePath = filePath;
                        _blockManager = new BlockManager(new BlockModifier(filePath));
                }

                public uint FreeFiles
                {
                        get { return _blockManager.SuperBlock.FreeIndexNodes; }
                }

                public long FreeSpace
                {
                        get { return _blockManager.SuperBlock.FreeBlocks * Block.SIZE; }
                }

                public long TotalSpace
                {
                        get { return _blockManager.SuperBlock.TotalBlocks * Block.SIZE; }
                }

                public uint CreateFile(long sizeInBytes)
                {
                        if (sizeInBytes <= 0)
                        {
                                // STATEMENT COVERAGE: Cannot write a test for this statement.
                                throw new ArgumentException("file size must be larger than 0");
                        }

                        uint size = Block.BlocksNeeded(sizeInBytes);

                        uint newFileNumber = _blockManager.SuperBlock.GetFreeIndexNode();

                        uint indexNodeBlockIndex = IndexNode.CalculateBlock(newFileNumber);
                        uint indexNodeBlockRelativeIndex = IndexNode.CalculateBlockRelativeIndex(newFileNumber);

                        var indexNodeBlock = _blockManager.Request<IndexNodesBlock>(indexNodeBlockIndex);

                        IndexNode newIndexNode = indexNodeBlock.IndexNodes[indexNodeBlockRelativeIndex];

                        for (int i = 0; i < size; i++)
                        {
                                try
                                {
                                        newIndexNode.AddDataBlock();
                                }
                                catch (OutOfFreeSpaceException)
                                {
                                        newIndexNode.FreeDataBlocks();
                                        _blockManager.DoneUsing(indexNodeBlock, true);

                                        _blockManager.SuperBlock.FreeIndexNode(newFileNumber);

                                        throw;
                                }
                        }

                        _blockManager.DoneUsing(indexNodeBlock, true);

                        // return file number
                        return newFileNumber;
                }


                public void DeleteFile(uint fileNumber)
                {
                        uint indexNodeBlockIndex = IndexNode.CalculateBlock(fileNumber);
                        uint indexNodeBlockRelativeIndex = IndexNode.CalculateBlockRelativeIndex(fileNumber);

                        var indexNodeBlock = _blockManager.Request<IndexNodesBlock>(indexNodeBlockIndex);

                        IndexNode indexNode = indexNodeBlock.IndexNodes[indexNodeBlockRelativeIndex];

                        indexNode.FreeDataBlocks();

                        _blockManager.DoneUsing(indexNodeBlock, true);

                        _blockManager.SuperBlock.FreeIndexNode(fileNumber);
                }

                public void ChangeFileSize(uint fileNumber, long newSizeInBytes)
                {
                        if (newSizeInBytes <= 0)
                        {
                                throw new ArgumentException("file size must be larger than 0");
                        }

                        uint newSize = Block.BlocksNeeded(newSizeInBytes);

                        uint indexNodeBlockIndex = IndexNode.CalculateBlock(fileNumber);
                        uint indexNodeBlockRelativeIndex = IndexNode.CalculateBlockRelativeIndex(fileNumber);

                        var indexNodeBlock = _blockManager.Request<IndexNodesBlock>(indexNodeBlockIndex);

                        IndexNode indexNode = indexNodeBlock.IndexNodes[indexNodeBlockRelativeIndex];

                        if (indexNode.DataBlocks < newSize)
                        {
                                uint blocksToAdd = newSize - indexNode.DataBlocks;

                                for (int i = 0; i < blocksToAdd; i++)
                                {
                                        indexNode.AddDataBlock();
                                }

                                _blockManager.DoneUsing(indexNodeBlock, true);
                        }
                        else if (indexNode.DataBlocks > newSize)
                        {
                                uint blocksToRemove = indexNode.DataBlocks - newSize;

                                for (int i = 0; i < blocksToRemove; i++)
                                {
                                        indexNode.RemoveDataBlock();
                                }

                                _blockManager.DoneUsing(indexNodeBlock, true);
                        }
                        else
                        {
                                _blockManager.DoneUsing(indexNodeBlock, false);
                        }
                }

                public FileStream OpenFile(uint fileNumber)
                {
                        return new FileStream(fileNumber, _blockManager);
                }

                public static ContainerFile Create(string fileName, long maximumSizeInBytes, uint maximumFiles)
                {
                        if (maximumSizeInBytes <= 0)
                        {
                                throw new InvalidDiskSizeException("maximum size must be larger than 0");
                        }

                        if (maximumFiles <= 0)
                        {
                                throw new InvalidFileCountException("maximum amount of files must be larger than 0");
                        }

                        uint totalBlocks = Block.BlocksNeeded(maximumSizeInBytes);
                        uint indexNodesBlocks = maximumFiles / IndexNode.INDEX_NODES_PER_BLOCK + 1;

                        if (indexNodesBlocks >= totalBlocks)
                        {
                                throw new InvalidFileCountException(string.Format("cannot manage up to {0} files with a maximum disk space of {1} bytes", maximumFiles, maximumSizeInBytes));
                        }

                        if (File.Exists(fileName))
                        {
                                File.Delete(fileName);
                        }


                        var freeBlocksStack = new Stack<uint>();
                        freeBlocksStack.Push(0);

                        var superBlock = new SuperBlock
                                {
                                        BlockIndex = SuperBlock.SUPER_BLOCK_LOCATION,
                                        TotalBlocks = totalBlocks,
                                        FreeBlocks = totalBlocks - 1 - indexNodesBlocks,
                                        HighestDataBlock = 1 + indexNodesBlocks,
                                        TotalInodes = indexNodesBlocks * (Block.SIZE / IndexNode.SIZE),
                                        FreeIndexNodes = indexNodesBlocks * (Block.SIZE / IndexNode.SIZE),
                                        IndexNodesBlocks = indexNodesBlocks,
                                        FreeIndexNodesQueueMaxSize = ((Block.SIZE - SuperBlock.STATIC_SIZE) / 4) / 4,
                                        FreeIndexNodesQueue = new Queue<uint>(),
                                        FreeBlockStackMaxSize = ((Block.SIZE - SuperBlock.STATIC_SIZE) / 4) / 4 * 3,
                                        FreeBlockStack = freeBlocksStack
                                };

                        var blockModifier = new BlockModifier(fileName);
                        blockModifier.WriteBlock(superBlock);

                        return new ContainerFile(fileName);
                }

                public static ContainerFile Open(string fileName)
                {
                        if (!File.Exists(fileName))
                        {
                                throw new FileNotFoundException();
                        }

                        return new ContainerFile(fileName);
                }

                public void Close()
                {
                        _blockManager.Flush();
                }

                public void Delete()
                {
                        File.Delete(_filePath);
                }
        }
}