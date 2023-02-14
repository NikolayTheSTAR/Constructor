using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheSTAR.World.Blocks
{
    // this block can be merged with family blocks
    public class FamilyBlock : MergableBlock
    {
        [SerializeField] private BlockType [] familyBlocks = new BlockType[0];

        public bool IsFamily(BlockType blockType) => Array.IndexOf(familyBlocks, blockType) != -1;
    }
}