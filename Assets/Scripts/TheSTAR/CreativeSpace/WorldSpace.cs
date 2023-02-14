using System;
using UnityEngine;
using TheSTAR.Utility;
using TheSTAR.World.Blocks;

namespace TheSTAR.CreativeSpace
{
     // CreativeSpace for game content blocks
    public class WorldSpace : CreativeSpace
    {
        [SerializeField] private Transform[] layers = new Transform[1];
        private Block[,,] worldBlocks = new Block[1, 1, 1];
        [SerializeField] private int interactionGridLayer = -1;
        protected override float CellsZPos => layers[0].position.z;

        public override int GetPoolWidth => worldBlocks.GetLength(0);
        public override int GetPoolHeight => worldBlocks.GetLength(1);
        
        public override void CreateBlock(Block prefab, int x, int y)
        {
            if (prefab == null || prefab.BlockType == BlockType.Ctor) return;
            
            x = Math.Abs(x);
            y = Math.Abs(y);
            var z = Controllers.BlockController.GetGridLayerForBlock(prefab.BlockType);
            
            // check old block
            
            var oldBlock = worldBlocks[x, y, z];
            if (oldBlock != null && oldBlock.BlockType == prefab.BlockType) return;
            ClearBlock(x, y, z, out _);
            
            // create block

            var cb = Instantiate(prefab, new Vector3(x, -y, CellsZPos), Quaternion.identity, layers[z]); // just created block
            cb.SetLayer(z);
            worldBlocks[x, y, z] = cb;

            // update visual

            if (cb is not IMergableBlock mb) return;
            
            var bt = cb.BlockType;

            IMergableBlock tb; // temp block (for check)
            var createdBlockGridPos = new IntVector2(x, y);
                
            UpdateMergeForDirection(WorldDirections.North);
            UpdateMergeForDirection(WorldDirections.South);
            UpdateMergeForDirection(WorldDirections.West);
            UpdateMergeForDirection(WorldDirections.East);
            
            var visualType = BlockController.GetBlockVisualType(mb.MergeUp, mb.MergeDown, mb.MergeLeft, mb.MergeRight);
            var sprite = BlockController.BlockVisual.GetSprite(cb.BlockType, visualType);

            mb.SetSprite(sprite);

            void UpdateMergeForDirection(WorldDirections d)
            {
                mb.SetMergeData(d, CanBeMerged(d));
                    
                bool CanBeMerged(WorldDirections offsetDirection)
                {
                    var offset = new IntVector2(offsetDirection);
                
                    if (!MathUtility.InBounds(createdBlockGridPos + offset, new IntVector2(0, 0), new IntVector2(UsedWidth - 1, UsedHeight - 1))) return false;
                
                    tb = worldBlocks[x + offset.X, y + offset.Y, z] as IMergableBlock;
                
                    if (tb == null) return false;
                    if (!TryMergeAsFamily(bt, tb)) return false;

                    tb.SetMergeData(MathUtility.ReverseWorldDirection(offsetDirection), true);
                
                    BlockController.UpdateVisualForBlock(tb);

                    return true;

                    bool TryMergeAsFamily(BlockType currentBlockType, IMergableBlock checkedBlock)
                    {
                        if (checkedBlock.BlockType == currentBlockType) return true;
                    
                        var fb = checkedBlock as FamilyBlock;
                        return fb != null && fb.IsFamily(currentBlockType);
                    }
                }
            }
        }
        
        protected override void UpdateSize()
        {
            int
            poolWidth = Mathf.Max(UsedWidth, GetPoolWidth),
            poolHeight = Mathf.Max(UsedHeight, GetPoolHeight);

            worldBlocks = ArrayUtility.UpdateArraySize(worldBlocks, poolWidth, poolHeight, layers.Length);
        }
        
        protected override void UpdateCellsActivityInPool()
        {
            var isBounds = false;
            Block block;
            
            for (var y = 0; y < worldBlocks.GetLength(1); y++)
            {
                for (var x = 0; x < worldBlocks.GetLength(0); x++)
                {
                    for (var z = 0; z < worldBlocks.GetLength(2); z++)
                    {
                        block = worldBlocks[x, y, z];
                        if (block == null) continue;
                        isBounds = x < UsedWidth && y < UsedHeight;
                        block.gameObject.SetActive(isBounds);
                    }
                }
            }
        }

        public void ResetInteractionGridLayer() => interactionGridLayer = -1;

        public override void ClearBlock(int x, int y)
        {
            bool wasDeleted;
            
            if (interactionGridLayer == -1)
            {
                for (var clearZ = worldBlocks.GetLength(2) - 1; clearZ >= 0; clearZ--)
                {
                    ClearBlock(x, y, clearZ, out wasDeleted);

                    if (!wasDeleted) continue;
                    interactionGridLayer = clearZ;
                    break;
                }
            }
            else ClearBlock(x, y, interactionGridLayer, out wasDeleted);
        }

        public override void ClearBlock(int x, int y, int z, out bool wasDeleted)
        {
            var oldBlock = worldBlocks[x, y, z];
            wasDeleted = false;

            if (oldBlock == null) return;

            // update neighbors

            if (oldBlock is IMergableBlock oldMb)
            {
                IMergableBlock oldNmb; // old neighbor mergable block
                
                if (oldMb.MergeUp)
                {
                    oldNmb = worldBlocks[x, y - 1, z] as IMergableBlock;
                    if (oldNmb != null)
                    {
                        oldNmb.SetMergeData(WorldDirections.South, false);
                        BlockController.UpdateVisualForBlock(oldNmb);
                    }
                }

                if (oldMb.MergeDown)
                {
                    oldNmb = worldBlocks[x, y + 1, z] as IMergableBlock;
                    if (oldNmb != null)
                    {
                        oldNmb.SetMergeData(WorldDirections.North, false);
                        BlockController.UpdateVisualForBlock(oldNmb);
                    }
                }

                if (oldMb.MergeLeft)
                {
                    oldNmb = worldBlocks[x - 1, y, z] as IMergableBlock;
                    if (oldNmb != null)
                    {
                        oldNmb.SetMergeData(WorldDirections.East, false);
                        BlockController.UpdateVisualForBlock(oldNmb);
                    }
                }

                if (oldMb.MergeRight)
                {
                    oldNmb = worldBlocks[x + 1, y, z] as IMergableBlock;
                    if (oldNmb != null)
                    {
                        oldNmb.SetMergeData(WorldDirections.West, false);
                        BlockController.UpdateVisualForBlock(oldNmb);
                    }
                }
            }

            // destroy
            
            Destroy(oldBlock.gameObject);
            worldBlocks[x, y, z] = null;
            wasDeleted = true;
        }

        public override void ClearPool(out int deletedCells)
        {
            deletedCells = 0;

            bool isBounds, wasDeleted;
            
            for (var z = 0; z < worldBlocks.GetLength(2); z++)
            {
                for (var y = 0; y < worldBlocks.GetLength(1); y++)
                {
                    for (var x = 0; x < worldBlocks.GetLength(0); x++)
                    {
                        isBounds = x < UsedWidth && y < UsedHeight;
                        if (isBounds) continue;
                        
                        ClearBlock(x, y, z, out wasDeleted);
                        if (wasDeleted) deletedCells++;
                    }
                }   
            }
        }
    }
}