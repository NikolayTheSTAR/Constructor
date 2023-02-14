using System;
using System.Threading.Tasks;
using TheSTAR.Controllers;
using TheSTAR.Utility;
using TheSTAR.World;
using TheSTAR.World.Blocks;
using UnityEngine;
using Zenject;

// space for working with game blocks (e.x. WorldSpace, GridSpace)
namespace TheSTAR.CreativeSpace
{
    public abstract class CreativeSpace : MonoBehaviour, IResetable
    {
        protected int usedWidth;
        protected int usedHeight;
        
        [Inject] protected BlockController BlockController;
        
        public int UsedWidth => usedWidth;
        public int UsedHeight => usedHeight;
        protected abstract float CellsZPos { get; }
        public abstract int GetPoolWidth { get; }
        public abstract int GetPoolHeight { get; }
        
        public int GetHeight => usedHeight;
        public int GetWidth => usedWidth;

        public const int MinSize = 1;
        public const int MaxSize = 100;
        public const int DefaultSize = 10;

        #region Size

        public void AddSize(int value) => AddSize(value, value);
        private void AddSize(int width, int height) => SetSize(usedWidth + width, usedHeight + height);
        private void SetSize(int value) => SetSize(value, value);
        public void SetSize(int width, int height)
        {
            usedWidth = MathUtility.Limit(width, MinSize, MaxSize);
            usedHeight = MathUtility.Limit(height, MinSize, MaxSize);

            UpdateCellsActivityInPool();
            UpdateSize();
        }
        
        protected abstract void UpdateCellsActivityInPool();

        protected abstract void UpdateSize();

        #endregion

        public void Reset() => SetSize(DefaultSize);
        
        public abstract void CreateBlock(Block prefab, int gridPosX, int gridPosY);

        public abstract void ClearBlock(int x, int y);

        public abstract void ClearBlock(int x, int y, int z, out bool wasDeleted);

        public abstract void ClearPool(out int deletedCells);
    }
}