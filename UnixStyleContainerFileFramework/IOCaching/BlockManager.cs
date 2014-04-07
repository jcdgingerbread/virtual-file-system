using System;
using System.Collections.Generic;
using System.IO;
using UnixStyleContainerFileFramework.Blocks;

namespace UnixStyleContainerFileFramework.IOCaching
{
        internal class BlockManager
        {
                private const uint CACHE_SIZE = 5;

                private readonly BlockModifier _blockModifier;

                /// <summary>
                ///         mapping from block id to block in memory.
                /// </summary>
                private readonly Dictionary<uint, CacheItem> _cache;

                private readonly LinkedList<CacheItem> _unused;
                private readonly Dictionary<uint, LinkedListNode<CacheItem>> _unusedMap;


                public BlockManager(BlockModifier blockModifier)
                {
                        _blockModifier = blockModifier;
                        _cache = new Dictionary<uint, CacheItem>();
                        _unused = new LinkedList<CacheItem>();
                        _unusedMap = new Dictionary<uint, LinkedListNode<CacheItem>>();

                        SuperBlock = _blockModifier.ReadBlock<SuperBlock>(SuperBlock.SUPER_BLOCK_LOCATION, this);
                }

                public SuperBlock SuperBlock { get; private set; }

                public T Request<T>(uint block) where T : Block, new()
                {
                        if (block == SuperBlock.SUPER_BLOCK_LOCATION)
                        {
                                throw new ArgumentException("super block should be accessed with property");
                        }

                        if (!_cache.ContainsKey(block))
                        {
                                if (_cache.Count >= CACHE_SIZE)
                                {
                                        if (_unusedMap.Count > 0)
                                        {
                                                CacheItem blockToEvict = _unused.First.Value;

                                                if (blockToEvict.Dirty)
                                                {
                                                        WriteBlock(blockToEvict.Block);

                                                        blockToEvict.Dirty = false;
                                                }

                                                Remove(blockToEvict.Block.BlockIndex);
                                        }
                                        else
                                        {
                                                throw new InsufficientMemoryException("all cached blocks in use and no more free cache space");
                                        }
                                }

                                _cache.Add(block, new CacheItem(_blockModifier.ReadBlock<T>(block, this)));
                        }
                        else if (_cache[block].Usage == 0)
                        {
                                LinkedListNode<CacheItem> nodeToRemove = _unusedMap[block];

                                _unused.Remove(nodeToRemove);
                                _unusedMap.Remove(block);
                        }

                        CacheItem requestedCacheItem = _cache[block];
                        requestedCacheItem.Usage++;

                        var result = requestedCacheItem.Block as T;
                        if (result == null)
                        {
                                throw new InvalidDataException("file corrupted");
                        }

                        return result;
                }

                public void Flush()
                {
                        foreach (CacheItem block in _cache.Values)
                        {
                                if (!block.Dirty) continue;

                                WriteBlock(block.Block);
                                block.Dirty = false;
                        }

                        WriteBlock(SuperBlock);
                }

                public void WriteBlock(Block block)
                {
                        _blockModifier.WriteBlock(block);
                }

                public void Remove(uint block)
                {
                        if (_cache.ContainsKey(block))
                        {
                                if (_unusedMap.ContainsKey(block))
                                {
                                        LinkedListNode<CacheItem> nodeToRemove = _unusedMap[block];

                                        _unused.Remove(nodeToRemove);
                                        _unusedMap.Remove(block);
                                }

                                _cache.Remove(block);
                        }
                }

                public void DoneUsing(Block block, bool dirty)
                {
                        if (!_cache.ContainsKey(block.BlockIndex))
                        {
                                throw new ArgumentException("block not managed");
                        }

                        CacheItem cacheItem = _cache[block.BlockIndex];

                        if (dirty) cacheItem.Dirty = true;

                        if (--cacheItem.Usage == 0)
                        {
                                _unusedMap.Add(block.BlockIndex, _unused.AddLast(cacheItem));
                        }
                }

                private class CacheItem
                {
                        public CacheItem(Block block)
                        {
                                Block = block;
                        }

                        public Block Block { get; private set; }

                        public bool Dirty { get; set; }

                        public uint Usage { get; set; }
                }
        }
}