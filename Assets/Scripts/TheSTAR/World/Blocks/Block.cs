using UnityEngine;
using UnityEngine.Serialization;

namespace TheSTAR.World.Blocks
{
    // Cell in game world, e.g. cell of grass, water, stone
    public class Block : MonoBehaviour, ITypedBlock
    {
        [SerializeField] private BlockType blockType;
        [SerializeField] protected SpriteRenderer sr;

        public BlockType BlockType => blockType;

        public override string ToString() => name;

        public void SetLayer(int layerIndex) => sr.sortingOrder = layerIndex;

        public int GetZPos => sr.sortingOrder;
    }
}