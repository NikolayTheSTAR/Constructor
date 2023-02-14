using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Data/BlockVisual", fileName = "BlockVisualConfig")]
public class BlockVisualConfig : ScriptableObject
{
    [SerializeField] private BlockVisualData[] _blockDatas = new BlockVisualData[0];

    public Sprite GetSprite(BlockType _blockType, BlockVisualType _visualType)
    {
        var blockData = Array.Find(_blockDatas, (info) => info.BlockType == _blockType);
        if (blockData == null) return null;

        return blockData.GetSprite(_visualType);
    }

    [Serializable]
    public class BlockVisualData : IComparable<BlockVisualData>
    {
        [SerializeField] private BlockType _blockType;
        public BlockType BlockType => _blockType;
        [SerializeField] private bool _useAdaptivity;
        [SerializeField, HideIf("_useAdaptivity")] private Sprite _singleSprite;
        [SerializeField, ShowIf("_useAdaptivity")] private Sprite[] _adaptiveSprites = new Sprite[0];

        public Sprite GetSprite(BlockVisualType visualType)
        {
            if (_useAdaptivity) return _adaptiveSprites[(int)visualType];
            else return _singleSprite;
        }

        public int CompareTo(BlockVisualData other)
        {
            return (int)_blockType - (int)other._blockType;
        }
    }
}

public enum BlockVisualType
{
    MergeDR = 0,
    BoundU,
    MergeDL,
    BoundL,
    Full = 4,
    BoundR,
    MergeUR,
    BoundD,
    MergeUL,
    MergeR,
    MergeLR,
    MergeL,
    MergeD,
    MergeUD,
    MergeU,
    Round = 15
}