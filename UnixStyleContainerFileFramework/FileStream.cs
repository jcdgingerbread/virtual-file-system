using System;
using System.IO;
using UnixStyleContainerFileFramework.Blocks;
using UnixStyleContainerFileFramework.IOCaching;
using UnixStyleContainerFileFramework.Structural;

namespace UnixStyleContainerFileFramework
{
        public class FileStream
        {
                private readonly BlockManager _blockManager;
                private readonly uint _indexNodeBlockIndex;
                private readonly uint _indexNodeBlockRelativeIndex;

                internal FileStream(uint fileNumber, BlockManager blockManager)
                {
                        _blockManager = blockManager;
                        _indexNodeBlockIndex = IndexNode.CalculateBlock(fileNumber);
                        _indexNodeBlockRelativeIndex = IndexNode.CalculateBlockRelativeIndex(fileNumber);
                }

                public long Length
                {
                        get
                        {
                                var indexNodeBlock = _blockManager.Request<IndexNodesBlock>(_indexNodeBlockIndex);

                                IndexNode indexNode = indexNodeBlock.IndexNodes[_indexNodeBlockRelativeIndex];

                                uint result = indexNode.DataBlocks * Block.SIZE;

                                _blockManager.DoneUsing(indexNodeBlock, false);

                                return result;
                        }
                }

                public long Position { get; private set; }

                public void Seek(long change, SeekOrigin origin)
                {
                        Position = GetTargetPosition(change, origin);
                }

                private long GetTargetPosition(long change, SeekOrigin origin)
                {
                        switch (origin)
                        {
                                case SeekOrigin.Begin:
                                        if (change < 0 || Length < change)
                                        {
                                                throw new ArgumentException("target position out of range");
                                        }
                                        return change;
                                case SeekOrigin.Current:
                                        if (change + Position < 0 || Length < change + Position)
                                        {
                                                throw new ArgumentException("target position out of range");
                                        }

                                        return change + Position;
                                case SeekOrigin.End:
                                        if (change < -Length || 0 < change)
                                        {
                                                throw new ArgumentException("target position out of range");
                                        }

                                        return change + Length;
                                default:
                                        throw new ArgumentOutOfRangeException("origin");
                        }
                }

                public byte[] Read(long amount)
                {
                        if (Position + amount > Length)
                        {
                                throw new ArgumentException("read would exceed file size");
                        }

                        var indexNodeBlock = _blockManager.Request<IndexNodesBlock>(_indexNodeBlockIndex);

                        IndexNode indexNode = indexNodeBlock.IndexNodes[_indexNodeBlockRelativeIndex];

                        long dataLeftToRead = amount;
                        var stream = new MemoryStream();

                        while (dataLeftToRead > 0)
                        {
                                var relativeDataBlock = (uint) (Position / Block.SIZE);
                                var relativeDataBlockOffset = (int) (Position % Block.SIZE);

                                int dataLeftInCurrentBlock = Block.SIZE - relativeDataBlockOffset;

                                var dataToReadFromCurrentBlock = (int) (dataLeftInCurrentBlock < dataLeftToRead ? dataLeftInCurrentBlock : dataLeftToRead);

                                uint dataBlockIndex = indexNode.GetDataBlock(relativeDataBlock);

                                var dataBlock = _blockManager.Request<DataBlock>(dataBlockIndex);

                                stream.Write(dataBlock.Data, relativeDataBlockOffset, dataToReadFromCurrentBlock);

                                _blockManager.DoneUsing(dataBlock, false);

                                dataLeftToRead -= dataToReadFromCurrentBlock;

                                Position += dataToReadFromCurrentBlock;
                        }

                        _blockManager.DoneUsing(indexNodeBlock, false);

                        return stream.ToArray();
                }

                public void Write(byte[] data)
                {
                        if (data == null) return;

                        if (Position + data.Length > Length)
                        {
                                throw new ArgumentException("write would exceed file size");
                        }

                        var indexNodeBlock = _blockManager.Request<IndexNodesBlock>(_indexNodeBlockIndex);

                        IndexNode indexNode = indexNodeBlock.IndexNodes[_indexNodeBlockRelativeIndex];

                        var stream = new MemoryStream(data);

                        while (stream.Position < stream.Length)
                        {
                                long dataLeftToWrite = stream.Length - stream.Position;

                                var relativeDataBlock = (uint) (Position / Block.SIZE);
                                var relativeDataBlockOffset = (int) (Position % Block.SIZE);

                                int spaceLeftInCurrentBlock = Block.SIZE - relativeDataBlockOffset;

                                var dataToWriteToCurrentBlock = (int) (spaceLeftInCurrentBlock < dataLeftToWrite ? spaceLeftInCurrentBlock : dataLeftToWrite);

                                uint dataBlockIndex = indexNode.GetDataBlock(relativeDataBlock);

                                var dataBlock = _blockManager.Request<DataBlock>(dataBlockIndex);

                                var buffer = new byte[dataToWriteToCurrentBlock];
                                stream.Read(buffer, 0, dataToWriteToCurrentBlock);

                                Array.Copy(buffer, 0, dataBlock.Data, relativeDataBlockOffset, dataToWriteToCurrentBlock);

                                _blockManager.DoneUsing(dataBlock, true);

                                Position += dataToWriteToCurrentBlock;
                        }

                        _blockManager.DoneUsing(indexNodeBlock, false);
                }
        }
}