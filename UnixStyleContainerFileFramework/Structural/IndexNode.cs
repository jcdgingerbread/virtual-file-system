using System;
using UnixStyleContainerFileFramework.Blocks;
using UnixStyleContainerFileFramework.IOCaching;
using UnixStyleContainerFileFramework.Utils;

namespace UnixStyleContainerFileFramework.Structural
{
        internal class IndexNode
        {
                public const uint SIZE = 8 * 4;

                public const uint INDEX_NODES_PER_BLOCK = Block.SIZE / SIZE;

                private const uint DIRECT_POINTER_DATA_BLOCKS = 4;
                private const uint SINGLE_INDIRECT_POINTER_DATA_BLOCKS = PointerBlock.POINTERS;
                private const uint DOUBLE_INDIRECT_POINTER_DATA_BLOCKS = SINGLE_INDIRECT_POINTER_DATA_BLOCKS * SINGLE_INDIRECT_POINTER_DATA_BLOCKS;
                private const uint TRIPLE_INDIRECT_POINTER_DATA_BLOCKS = DOUBLE_INDIRECT_POINTER_DATA_BLOCKS * SINGLE_INDIRECT_POINTER_DATA_BLOCKS;
                private readonly BlockManager _blockManager;

                private IndexNode(BlockManager blockManager)
                {
                        _blockManager = blockManager;
                }

                public uint DataBlocks { get; private set; }
                private uint DirectPointer1 { get; set; }
                private uint DirectPointer2 { get; set; }
                private uint DirectPointer3 { get; set; }
                private uint DirectPointer4 { get; set; }
                private uint SingleIndirectPointer { get; set; }
                private uint DoubleIndirectPointer { get; set; }
                private uint TripleIndirectPointer { get; set; }


                public static uint CalculateBlock(uint fileNumber)
                {
                        return fileNumber / INDEX_NODES_PER_BLOCK + IndexNodesBlock.CATEGORY_FIRST_BLOCK_INDEX;
                }

                public static uint CalculateBlockRelativeIndex(uint fileNumber)
                {
                        return fileNumber % INDEX_NODES_PER_BLOCK;
                }

                public static IndexNode Deserialize(DataReader dataReader, BlockManager blockManager)
                {
                        return new IndexNode(blockManager)
                                {
                                        DataBlocks = dataReader.ReadUInt32(),
                                        DirectPointer1 = dataReader.ReadUInt32(),
                                        DirectPointer2 = dataReader.ReadUInt32(),
                                        DirectPointer3 = dataReader.ReadUInt32(),
                                        DirectPointer4 = dataReader.ReadUInt32(),
                                        SingleIndirectPointer = dataReader.ReadUInt32(),
                                        DoubleIndirectPointer = dataReader.ReadUInt32(),
                                        TripleIndirectPointer = dataReader.ReadUInt32()
                                };
                }

                public void Serialize(DataWriter dataWriter)
                {
                        dataWriter.WriteUInt32(DataBlocks);
                        dataWriter.WriteUInt32(DirectPointer1);
                        dataWriter.WriteUInt32(DirectPointer2);
                        dataWriter.WriteUInt32(DirectPointer3);
                        dataWriter.WriteUInt32(DirectPointer4);
                        dataWriter.WriteUInt32(SingleIndirectPointer);
                        dataWriter.WriteUInt32(DoubleIndirectPointer);
                        dataWriter.WriteUInt32(TripleIndirectPointer);
                }

                public void AddDataBlock()
                {
                        SetDataBlock(DataBlocks, _blockManager.SuperBlock.GetFreeDataBlock());

                        DataBlocks++;
                }

                public void RemoveDataBlock()
                {
                        FreeDataBlock(--DataBlocks);
                }

                private void SetDataBlock(uint dataBlock, uint value)
                {
                        if (dataBlock < DIRECT_POINTER_DATA_BLOCKS)
                        {
                                SetDirectPointerDataBlock(dataBlock, value);
                        }
                        else if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                                 SINGLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                SetSingleIndirectPointerDataBlock(dataBlock -
                                                                  DIRECT_POINTER_DATA_BLOCKS,
                                                                  value);
                        }
                        else if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                                 SINGLE_INDIRECT_POINTER_DATA_BLOCKS +
                                 DOUBLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                AddDoubleIndirectPointerDataBlock(dataBlock -
                                                                  DIRECT_POINTER_DATA_BLOCKS -
                                                                  SINGLE_INDIRECT_POINTER_DATA_BLOCKS,
                                                                  value);
                        }
                        else if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                                             SINGLE_INDIRECT_POINTER_DATA_BLOCKS +
                                 DOUBLE_INDIRECT_POINTER_DATA_BLOCKS +
                                 TRIPLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                AddTripleIndirectPointerDataBlock(dataBlock -
                                                                  DIRECT_POINTER_DATA_BLOCKS -
                                                                  SINGLE_INDIRECT_POINTER_DATA_BLOCKS -
                                                                  DOUBLE_INDIRECT_POINTER_DATA_BLOCKS,
                                                                  value);
                        }
                        else
                        {
                                throw new ArgumentOutOfRangeException("dataBlock");
                        }
                }

                private void SetDirectPointerDataBlock(uint dataBlock, uint value)
                {
                        switch (dataBlock)
                        {
                                case 0:
                                        DirectPointer1 = value;
                                        break;
                                case 1:
                                        DirectPointer2 = value;
                                        break;
                                case 2:
                                        DirectPointer3 = value;
                                        break;
                                case 3:
                                        DirectPointer4 = value;
                                        break;
                                default:
                                        throw new ArgumentOutOfRangeException("dataBlock");
                        }
                }

                private void SetSingleIndirectPointerDataBlock(uint relativeDataBlock, uint value)
                {
                        if (SingleIndirectPointer == 0)
                        {
                                SingleIndirectPointer = _blockManager.SuperBlock.GetFreeDataBlock();
                        }


                        var singleIndirectPointerBlock = _blockManager.Request<PointerBlock>(SingleIndirectPointer);

                        singleIndirectPointerBlock.DataBlocks[relativeDataBlock] = value;

                        _blockManager.DoneUsing(singleIndirectPointerBlock, true);
                }

                private void AddDoubleIndirectPointerDataBlock(uint relativeDataBlock, uint value)
                {
                        if (DoubleIndirectPointer == 0)
                        {
                                DoubleIndirectPointer = _blockManager.SuperBlock.GetFreeDataBlock();
                        }

                        var doubleIndirectFirstPointerBlock = _blockManager.Request<PointerBlock>(DoubleIndirectPointer);


                        uint indirectBlock = relativeDataBlock / SINGLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (doubleIndirectFirstPointerBlock.DataBlocks[indirectBlock] == 0)
                        {
                                var doubleIndirectFirstPointerBlockWa = _blockManager.Request<PointerBlock>(DoubleIndirectPointer);

                                doubleIndirectFirstPointerBlockWa.DataBlocks[indirectBlock] = _blockManager.SuperBlock.GetFreeDataBlock();

                                _blockManager.DoneUsing(doubleIndirectFirstPointerBlockWa, true);
                        }
                        uint offsetSecondDoubleIndirect = relativeDataBlock % PointerBlock.POINTERS;

                        var doubleIndirectSecondPointerBlock = _blockManager.Request<PointerBlock>(doubleIndirectFirstPointerBlock.DataBlocks[indirectBlock]);

                        _blockManager.DoneUsing(doubleIndirectFirstPointerBlock, false);

                        doubleIndirectSecondPointerBlock.DataBlocks[offsetSecondDoubleIndirect] = value;

                        _blockManager.DoneUsing(doubleIndirectSecondPointerBlock, true);
                }

                private void AddTripleIndirectPointerDataBlock(uint relativeDataBlock, uint value)
                {
                        if (TripleIndirectPointer == 0)
                        {
                                TripleIndirectPointer = _blockManager.SuperBlock.GetFreeDataBlock();
                        }

                        var tripleIndirectFirstPointerBlock = _blockManager.Request<PointerBlock>(TripleIndirectPointer);


                        uint firstIndirectBlock = relativeDataBlock / DOUBLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock] == 0)
                        {
                                var tripleIndirectFirstPointerBlockWa = _blockManager.Request<PointerBlock>(TripleIndirectPointer);

                                tripleIndirectFirstPointerBlockWa.DataBlocks[firstIndirectBlock] = _blockManager.SuperBlock.GetFreeDataBlock();

                                _blockManager.DoneUsing(tripleIndirectFirstPointerBlockWa, true);
                        }
                        uint offsetSecondTripleIndirect = relativeDataBlock % DOUBLE_INDIRECT_POINTER_DATA_BLOCKS;

                        var tripleIndirectSecondPointerBlock = _blockManager.Request<PointerBlock>(tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock]);

                        _blockManager.DoneUsing(tripleIndirectFirstPointerBlock, false);

                        uint secondIndirectBlock = offsetSecondTripleIndirect / SINGLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (tripleIndirectSecondPointerBlock.DataBlocks[secondIndirectBlock] == 0)
                        {
                                var tripleIndirectSecondPointerBlockWa = _blockManager.Request<PointerBlock>(tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock]);

                                tripleIndirectSecondPointerBlockWa.DataBlocks[secondIndirectBlock] = _blockManager.SuperBlock.GetFreeDataBlock();

                                _blockManager.DoneUsing(tripleIndirectSecondPointerBlockWa, true);
                        }

                        uint offsetSecondDoubleIndirect = offsetSecondTripleIndirect % PointerBlock.POINTERS;

                        var tripleIndirectThirdPointerBlock = _blockManager.Request<PointerBlock>(tripleIndirectSecondPointerBlock.DataBlocks[secondIndirectBlock]);

                        _blockManager.DoneUsing(tripleIndirectSecondPointerBlock, false);

                        tripleIndirectThirdPointerBlock.DataBlocks[offsetSecondDoubleIndirect] = value;

                        _blockManager.DoneUsing(tripleIndirectThirdPointerBlock, true);
                }

                public uint GetDataBlock(uint dataBlock)
                {
                        if (dataBlock < DIRECT_POINTER_DATA_BLOCKS)
                        {
                                return GetDirectPointerDataBlock(dataBlock);
                        }

                        if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                            SINGLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                return GetSingleIndirectPointerDataBlock(dataBlock -
                                                                         DIRECT_POINTER_DATA_BLOCKS);
                        }

                        if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                            SINGLE_INDIRECT_POINTER_DATA_BLOCKS +
                            DOUBLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                return GetDoubleIndirectPointerDataBlock(dataBlock -
                                                                         DIRECT_POINTER_DATA_BLOCKS -
                                                                         SINGLE_INDIRECT_POINTER_DATA_BLOCKS);
                        }

                        if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                                        SINGLE_INDIRECT_POINTER_DATA_BLOCKS +
                            DOUBLE_INDIRECT_POINTER_DATA_BLOCKS +
                            TRIPLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                return GetTripleIndirectPointerDataBlock(dataBlock -
                                                                         DIRECT_POINTER_DATA_BLOCKS -
                                                                         SINGLE_INDIRECT_POINTER_DATA_BLOCKS -
                                                                         DOUBLE_INDIRECT_POINTER_DATA_BLOCKS);
                        }

                        throw new ArgumentOutOfRangeException("dataBlock");
                }

                private uint GetDirectPointerDataBlock(uint relativeDataBlock)
                {
                        switch (relativeDataBlock)
                        {
                                case 0:
                                        return DirectPointer1;
                                case 1:
                                        return DirectPointer2;
                                case 2:
                                        return DirectPointer3;
                                case 3:
                                        return DirectPointer4;
                                default:
                                        throw new ArgumentOutOfRangeException("relativeDataBlock");
                        }
                }

                private uint GetSingleIndirectPointerDataBlock(uint relativeDataBlock)
                {
                        if (SingleIndirectPointer == 0)
                        {
                                return 0;
                        }

                        var singleIndirectPointerBlock = _blockManager.Request<PointerBlock>(SingleIndirectPointer);

                        uint result = singleIndirectPointerBlock.DataBlocks[relativeDataBlock];

                        _blockManager.DoneUsing(singleIndirectPointerBlock, false);

                        return result;
                }

                private uint GetDoubleIndirectPointerDataBlock(uint relativeDataBlock)
                {
                        if (DoubleIndirectPointer == 0)
                        {
                                return 0;
                        }

                        var doubleIndirectFirstPointerBlock = _blockManager.Request<PointerBlock>(DoubleIndirectPointer);


                        uint indirectBlock = relativeDataBlock / SINGLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (doubleIndirectFirstPointerBlock.DataBlocks[indirectBlock] == 0)
                        {
                                _blockManager.DoneUsing(doubleIndirectFirstPointerBlock, false);

                                return 0;
                        }
                        uint offsetSecondDoubleIndirect = relativeDataBlock % PointerBlock.POINTERS;

                        var doubleIndirectSecondPointerBlock = _blockManager.Request<PointerBlock>(doubleIndirectFirstPointerBlock.DataBlocks[indirectBlock]);

                        _blockManager.DoneUsing(doubleIndirectFirstPointerBlock, false);

                        uint result = doubleIndirectSecondPointerBlock.DataBlocks[offsetSecondDoubleIndirect];

                        _blockManager.DoneUsing(doubleIndirectSecondPointerBlock, false);

                        return result;
                }

                private uint GetTripleIndirectPointerDataBlock(uint relativeDataBlock)
                {
                        if (TripleIndirectPointer == 0)
                        {
                                return 0;
                        }

                        var tripleIndirectFirstPointerBlock = _blockManager.Request<PointerBlock>(TripleIndirectPointer);


                        uint firstIndirectBlock = relativeDataBlock / DOUBLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock] == 0)
                        {
                                _blockManager.DoneUsing(tripleIndirectFirstPointerBlock, false);

                                return 0;
                        }
                        uint offsetSecondTripleIndirect = relativeDataBlock % DOUBLE_INDIRECT_POINTER_DATA_BLOCKS;

                        var tripleIndirectSecondPointerBlock = _blockManager.Request<PointerBlock>(tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock]);

                        _blockManager.DoneUsing(tripleIndirectFirstPointerBlock, false);

                        uint secondIndirectBlock = offsetSecondTripleIndirect / SINGLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (tripleIndirectSecondPointerBlock.DataBlocks[secondIndirectBlock] == 0)
                        {
                                _blockManager.DoneUsing(tripleIndirectSecondPointerBlock, false);

                                return 0;
                        }
                        uint offsetSecondDoubleIndirect = offsetSecondTripleIndirect % PointerBlock.POINTERS;

                        var tripleIndirectThirdPointerBlock = _blockManager.Request<PointerBlock>(tripleIndirectSecondPointerBlock.DataBlocks[secondIndirectBlock]);

                        _blockManager.DoneUsing(tripleIndirectSecondPointerBlock, false);

                        uint result = tripleIndirectThirdPointerBlock.DataBlocks[offsetSecondDoubleIndirect];

                        _blockManager.DoneUsing(tripleIndirectThirdPointerBlock, false);

                        return result;
                }

                public void FreeDataBlocks()
                {
                        while (DataBlocks > 0)
                        {
                                RemoveDataBlock();
                        }
                }

                private void FreeDataBlock(uint dataBlock)
                {
                        if (dataBlock < DIRECT_POINTER_DATA_BLOCKS)
                        {
                                FreeDirectPointerDataBlock(dataBlock);
                        }
                        else if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                                 SINGLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                FreeSingleIndirectPointerDataBlock(dataBlock -
                                                                   DIRECT_POINTER_DATA_BLOCKS);
                        }
                        else if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                                 SINGLE_INDIRECT_POINTER_DATA_BLOCKS +
                                 DOUBLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                FreeDoubleIndirectPointerDataBlock(dataBlock -
                                                                   DIRECT_POINTER_DATA_BLOCKS -
                                                                   SINGLE_INDIRECT_POINTER_DATA_BLOCKS);
                        }
                        else if (dataBlock < DIRECT_POINTER_DATA_BLOCKS +
                                             SINGLE_INDIRECT_POINTER_DATA_BLOCKS +
                                 DOUBLE_INDIRECT_POINTER_DATA_BLOCKS +
                                 TRIPLE_INDIRECT_POINTER_DATA_BLOCKS)
                        {
                                FreeTripleIndirectPointerDataBlock(dataBlock -
                                                                   DIRECT_POINTER_DATA_BLOCKS -
                                                                   SINGLE_INDIRECT_POINTER_DATA_BLOCKS -
                                                                   DOUBLE_INDIRECT_POINTER_DATA_BLOCKS);
                        }
                        else
                        {
                                throw new ArgumentOutOfRangeException("dataBlock");
                        }
                }

                private void FreeDirectPointerDataBlock(uint relativeDataBlock)
                {
                        uint blockToFree = GetDataBlock(relativeDataBlock);

                        if (blockToFree == 0) return;

                        _blockManager.SuperBlock.FreeDataBlock(blockToFree);

                        SetDataBlock(relativeDataBlock, 0);
                }

                private void FreeSingleIndirectPointerDataBlock(uint relativeDataBlock)
                {
                        if (SingleIndirectPointer == 0) return;

                        var singleIndirectPointerBlock = _blockManager.Request<PointerBlock>(SingleIndirectPointer);

                        uint blockToFree = singleIndirectPointerBlock.DataBlocks[relativeDataBlock];

                        if (blockToFree == 0) return;

                        _blockManager.SuperBlock.FreeDataBlock(blockToFree);

                        singleIndirectPointerBlock.DataBlocks[relativeDataBlock] = 0;

                        _blockManager.DoneUsing(singleIndirectPointerBlock, true);

                        if (relativeDataBlock == 0)
                        {
                                _blockManager.SuperBlock.FreeDataBlock(SingleIndirectPointer);

                                SingleIndirectPointer = 0;
                        }
                }

                private void FreeDoubleIndirectPointerDataBlock(uint relativeDataBlock)
                {
                        if (DoubleIndirectPointer == 0) return;

                        var doubleIndirectFirstPointerBlock = _blockManager.Request<PointerBlock>(DoubleIndirectPointer);


                        uint indirectBlock = relativeDataBlock / SINGLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (doubleIndirectFirstPointerBlock.DataBlocks[indirectBlock] == 0)
                        {
                                _blockManager.DoneUsing(doubleIndirectFirstPointerBlock, false);

                                return;
                        }

                        uint offsetSecondDoubleIndirect = relativeDataBlock % PointerBlock.POINTERS;

                        var doubleIndirectSecondPointerBlock = _blockManager.Request<PointerBlock>(doubleIndirectFirstPointerBlock.DataBlocks[indirectBlock]);


                        uint blockToFree = doubleIndirectSecondPointerBlock.DataBlocks[offsetSecondDoubleIndirect];

                        if (blockToFree == 0)
                        {
                                _blockManager.DoneUsing(doubleIndirectFirstPointerBlock, false);
                                _blockManager.DoneUsing(doubleIndirectSecondPointerBlock, false);

                                return;
                        }

                        _blockManager.SuperBlock.FreeDataBlock(blockToFree);

                        doubleIndirectSecondPointerBlock.DataBlocks[offsetSecondDoubleIndirect] = 0;

                        _blockManager.DoneUsing(doubleIndirectSecondPointerBlock, true);

                        if (offsetSecondDoubleIndirect == 0)
                        {
                                uint indirectBlockToRemove = doubleIndirectFirstPointerBlock.DataBlocks[indirectBlock];

                                _blockManager.SuperBlock.FreeDataBlock(indirectBlockToRemove);

                                doubleIndirectFirstPointerBlock.DataBlocks[indirectBlock] = 0;

                                _blockManager.DoneUsing(doubleIndirectFirstPointerBlock, true);
                        }
                        else
                        {
                                _blockManager.DoneUsing(doubleIndirectFirstPointerBlock, false);
                        }

                        if (indirectBlock == 0 && offsetSecondDoubleIndirect == 0)
                        {
                                _blockManager.SuperBlock.FreeDataBlock(DoubleIndirectPointer);

                                DoubleIndirectPointer = 0;
                        }
                }

                private void FreeTripleIndirectPointerDataBlock(uint relativeDataBlock)
                {
                        if (TripleIndirectPointer == 0) return;

                        var tripleIndirectFirstPointerBlock = _blockManager.Request<PointerBlock>(TripleIndirectPointer);


                        uint firstIndirectBlock = relativeDataBlock / DOUBLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock] == 0)
                        {
                                _blockManager.DoneUsing(tripleIndirectFirstPointerBlock, false);

                                return;
                        }
                        uint offsetSecondTripleIndirect = relativeDataBlock % DOUBLE_INDIRECT_POINTER_DATA_BLOCKS;

                        var tripleIndirectSecondPointerBlock = _blockManager.Request<PointerBlock>(tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock]);

                        uint secondIndirectBlock = offsetSecondTripleIndirect / SINGLE_INDIRECT_POINTER_DATA_BLOCKS;

                        if (tripleIndirectSecondPointerBlock.DataBlocks[secondIndirectBlock] == 0)
                        {
                                _blockManager.DoneUsing(tripleIndirectFirstPointerBlock, false);

                                _blockManager.DoneUsing(tripleIndirectSecondPointerBlock, false);

                                return;
                        }
                        uint offsetThirdTripleIndirect = offsetSecondTripleIndirect % PointerBlock.POINTERS;

                        var tripleIndirectThirdPointerBlock = _blockManager.Request<PointerBlock>(tripleIndirectSecondPointerBlock.DataBlocks[secondIndirectBlock]);

                        uint blockToFree = tripleIndirectThirdPointerBlock.DataBlocks[offsetThirdTripleIndirect];

                        if (blockToFree == 0)
                        {
                                _blockManager.DoneUsing(tripleIndirectFirstPointerBlock, false);

                                _blockManager.DoneUsing(tripleIndirectSecondPointerBlock, false);

                                _blockManager.DoneUsing(tripleIndirectThirdPointerBlock, false);

                                return;
                        }

                        _blockManager.SuperBlock.FreeDataBlock(blockToFree);

                        tripleIndirectThirdPointerBlock.DataBlocks[offsetThirdTripleIndirect] = 0;


                        _blockManager.DoneUsing(tripleIndirectThirdPointerBlock, true);

                        if (offsetThirdTripleIndirect == 0)
                        {
                                uint doubleIndirectBlockToFree = tripleIndirectSecondPointerBlock.DataBlocks[secondIndirectBlock];

                                _blockManager.SuperBlock.FreeDataBlock(doubleIndirectBlockToFree);

                                tripleIndirectSecondPointerBlock.DataBlocks[secondIndirectBlock] = 0;

                                _blockManager.DoneUsing(tripleIndirectSecondPointerBlock, true);
                        }
                        else
                        {
                                _blockManager.DoneUsing(tripleIndirectSecondPointerBlock, false);
                        }

                        if (secondIndirectBlock == 0 && offsetThirdTripleIndirect == 0)
                        {
                                uint indirectBlockToFree = tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock];

                                _blockManager.SuperBlock.FreeDataBlock(indirectBlockToFree);

                                tripleIndirectFirstPointerBlock.DataBlocks[firstIndirectBlock] = 0;

                                _blockManager.DoneUsing(tripleIndirectFirstPointerBlock, true);
                        }
                        else
                        {
                                _blockManager.DoneUsing(tripleIndirectFirstPointerBlock, false);
                        }

                        if (firstIndirectBlock == 0 && secondIndirectBlock == 0 && offsetThirdTripleIndirect == 0)
                        {
                                _blockManager.SuperBlock.FreeDataBlock(TripleIndirectPointer);

                                TripleIndirectPointer = 0;
                        }
                }
        }
}