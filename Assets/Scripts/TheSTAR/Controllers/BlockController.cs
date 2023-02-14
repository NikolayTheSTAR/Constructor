using TheSTAR.World;
using TheSTAR.World.Blocks;
using UnityEngine;
using Zenject;

namespace TheSTAR.Controllers
{
    public class BlockController : MonoBehaviour
    {
        private Block[] blockPrefabs = new Block[0];
    
        private const string ConfigPath = "Configs/BlockVisualConfig";
        private const string BlocksPath = "Prefabs/Blocks";

        private BlockVisualConfig _blockVisual;
        public BlockVisualConfig BlockVisual
        {
            get
            {
                if (_blockVisual == null) _blockVisual = Resources.Load<BlockVisualConfig>(ConfigPath);

                return _blockVisual;
            }
        }

        public void Init()
        {
            LoadBlockPrefabs();
        }

        [ContextMenu("LoadBlockPrefabs")]
        private void LoadBlockPrefabs()
        {
            var prefabs = Resources.LoadAll<Block>(BlocksPath);

            blockPrefabs = new Block[prefabs.Length + 1];

            foreach (var prefab in prefabs)
            {
                if (prefab == null) continue;
                blockPrefabs[(int)prefab.BlockType] = prefab;
            }
        }
    
        // update blocks visual by blocks merge data
        public void UpdateVisualForBlock(IMergableBlock mb)
        {
            var visualType = GetBlockVisualType(mb);
            var sprite = BlockVisual.GetSprite(mb.BlockType, visualType);
            mb.SetSprite(sprite);
        }

        private BlockVisualType GetBlockVisualType(IMergableBlock mb)
        {
            return GetBlockVisualType(mb.MergeUp, mb.MergeDown, mb.MergeLeft, mb.MergeRight);
        }

        public BlockVisualType GetBlockVisualType(bool mergeUp, bool mergeDown, bool mergeLeft, bool mergeRight)
        {
            BlockVisualType result;

            if (mergeUp)
            {
                if (mergeDown)
                {
                    if (mergeLeft) result = mergeRight ? BlockVisualType.Full : BlockVisualType.BoundR;
                    else result = mergeRight ? BlockVisualType.BoundL : BlockVisualType.MergeUD;
                }
                else
                {
                    if (mergeLeft) result = mergeRight ? BlockVisualType.BoundD : BlockVisualType.MergeUL;
                    else result = mergeRight ? BlockVisualType.MergeUR : BlockVisualType.MergeU;
                }
            }
            else
            {
                if (mergeDown)
                {
                    if (mergeLeft) result = mergeRight ? BlockVisualType.BoundU : BlockVisualType.MergeDL;
                    else result = mergeRight ? BlockVisualType.MergeDR : BlockVisualType.MergeD;
                }
                else
                {
                    if (mergeLeft) result = mergeRight ? BlockVisualType.MergeLR : BlockVisualType.MergeL;
                    else result = mergeRight ? BlockVisualType.MergeR : BlockVisualType.Round;
                }
            }

            return result;
        }
    
        public Block GetBlockPrefab(BlockType blockType)
        {
            var block = blockPrefabs[(int)blockType];
            if (block != null && block.BlockType != blockType) Debug.LogError("Block not found");

            return block;
        }

        public static int GetGridLayerForBlock(BlockType bt)
        {
            int result;

            switch (bt)
            {
                case BlockType.None:
                case BlockType.Earth:
                    result = 0;
                    break;

                case BlockType.EarthDark:
                case BlockType.Grass:
                case BlockType.StoneFloor:
                case BlockType.WoodFloor:
                case BlockType.Sand:
                    result = 1;
                    break;

                case BlockType.GrassDark:
                    result = 2;
                    break;

                case BlockType.Water:
                case BlockType.WaterWave:
                    result = 3;
                    break;

                default:
                    result = 4;
                    break;
            }

            return result;
        }
    }
}