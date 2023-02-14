using UnityEngine;

namespace TheSTAR.World.Blocks
{
    public class GrassBlock : Block, IMergableBlock
    {
        public bool MergeUp { get; private set; }
        public bool MergeDown { get; private set; }
        public bool MergeLeft { get; private set; }
        public bool MergeRight { get; private set; }

        public void SetSprite(Sprite sprite)
        {
            sr.sprite = sprite;
        }
        
        public void SetMergeData(WorldDirections direction, bool value)
        {
            switch (direction)
            {
                case WorldDirections.North:
                    MergeUp = value;
                    break;
                        
                case WorldDirections.South:
                    MergeDown = value;
                    break;
                        
                case WorldDirections.West:
                    MergeLeft = value;
                    break;
                        
                case WorldDirections.East:
                    MergeRight = value;
                    break;
            }
        }
    }
}