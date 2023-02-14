using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TheSTAR.World;
using TheSTAR.Utility;
using TheSTAR.World.Blocks;

namespace TheSTAR.CreativeSpace
{
    // special CreativeSpace for constructor cells
    public class GridSpace : CreativeSpace
    {
        [SerializeField] private Transform gridLayer;
        
        [Header("Lines")]
        [SerializeField] private Transform lineUp;
        [SerializeField] private Transform lineDown;
        [SerializeField] private Transform lineLeft;
        [SerializeField] private Transform lineRight;
        
        private const float PixelScale = 0.0625f;
        private const int CtorBlockOrderLayer = -1;
        
        private Block[,] _gridCells = new Block[1, 1];
        
        protected override float CellsZPos => gridLayer.position.z;
        public override int GetPoolWidth => _gridCells.GetLength(0);
        public override int GetPoolHeight => _gridCells.GetLength(1);

        protected override void UpdateCellsActivityInPool()
        {
            bool isBounds;
            Block block;

            for (var y = 0; y < _gridCells.GetLength(1); y++)
            {
                for (var x = 0; x < _gridCells.GetLength(0); x++)
                {
                    block = _gridCells[x, y];

                    if (block == null) continue;
                    isBounds = x < UsedWidth && y < UsedHeight;
                    block.gameObject.SetActive(isBounds);
                }
            }
        }

        protected override void UpdateSize()
        {
            int
            poolWidth = Mathf.Max(usedWidth, GetPoolWidth),
            poolHeight = Mathf.Max(usedHeight, GetPoolHeight);

            _gridCells = ArrayUtility.UpdateArraySize(_gridCells, poolWidth, poolHeight);

            // create ctor cells
             
            var z = CellsZPos;
            
            for (var y = 0; y < GetHeight; y++)
            {
                for (var x = 0; x < GetWidth; x++)
                {
                    if (_gridCells[x, y] == null) CreateBlock(BlockController.GetBlockPrefab(BlockType.Ctor), x, y);
                }
            }
            
            // bounds
            
            lineUp.localScale = new Vector3(usedWidth, PixelScale, 1);
            lineLeft.localScale = new Vector3(PixelScale, usedHeight, 1);
            lineDown.localScale = new Vector3(usedWidth, PixelScale, 1);
            lineRight.localScale = new Vector3(PixelScale, usedHeight + PixelScale, 1);

            lineDown.transform.localPosition = new Vector3(0, -usedHeight, 0);
            lineRight.transform.localPosition = new Vector3(usedWidth, 0, 0);
        }

        public override void CreateBlock(Block prefab, int x, int y)
        {
            if (prefab == null || prefab.BlockType != BlockType.Ctor) return;
            
            // check old block
            
            x = Math.Abs(x);
            y = Math.Abs(y);
            
            var oldBlock = _gridCells[x, y];

            if (oldBlock != null) return;
            
            // create block

            Block createdBlock;
            createdBlock = Instantiate(prefab, new Vector3(x, -y, CellsZPos), Quaternion.identity, gridLayer);
            _gridCells[x, y] = createdBlock;
            createdBlock.SetLayer(CtorBlockOrderLayer);
        }

        public override void ClearBlock(int x, int y)
        {
            // nothing
        }

        public override void ClearBlock(int x, int y, int z, out bool wasDeleted)
        {
            wasDeleted = false;
            
            var oldBlock = _gridCells[x, y];
            if (oldBlock == null) return;
            
            Destroy(oldBlock.gameObject);
            _gridCells[x, y] = null;
            wasDeleted = true;
        }
        
        public override void ClearPool(out int deletedCells)
        {
            deletedCells = 0;

            bool isBounds, wasDeleted;

            for (var y = 0; y < _gridCells.GetLength(1); y++)
            {
                for (var x = 0; x < _gridCells.GetLength(0); x++)
                {
                    isBounds = x < UsedWidth && y < UsedHeight;
                    if (isBounds) continue;
                    
                    ClearBlock(x, y, 0, out wasDeleted);
                    if (wasDeleted) deletedCells++;
                }
            }
        }
    }   
}