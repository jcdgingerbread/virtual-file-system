using System;
using System.IO;
using UnixStyleContainerFileFramework.Blocks;
using UnixStyleContainerFileFramework.Utils;

namespace UnixStyleContainerFileFramework.IOCaching
{
        internal class BlockModifier
        {
                private readonly string _filePath;

                public BlockModifier(string filePath)
                {
                        _filePath = filePath;
                }

                public T ReadBlock<T>(uint blockIndex, BlockManager blockManager) where T : Block, new()
                {
                        using (var reader = new BinaryReader(File.Open(_filePath, FileMode.Open)))
                        {
                                reader.BaseStream.Seek(blockIndex * Block.SIZE, SeekOrigin.Begin);

                                var block = new T {BlockIndex = blockIndex, BlockManager = blockManager};

                                var totalData = new byte[Block.SIZE];

                                byte[] readData = reader.ReadBytes(Block.SIZE);

                                Array.Copy(readData, totalData, readData.Length);

                                block.Deserialize(new DataReader(totalData));

                                return block;
                        }
                }

                public void WriteBlock(Block block)
                {
                        using (var writer = new BinaryWriter(File.Open(_filePath, FileMode.OpenOrCreate)))
                        {
                                writer.BaseStream.Seek(block.BlockIndex * Block.SIZE, SeekOrigin.Begin);

                                var dataWriter = new DataWriter();

                                block.Serialize(dataWriter);

                                writer.Write(dataWriter.GetData());
                        }
                }
        }
}