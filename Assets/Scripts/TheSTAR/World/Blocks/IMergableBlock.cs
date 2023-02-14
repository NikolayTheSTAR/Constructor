using TheSTAR.Interfaces;

namespace TheSTAR.World.Blocks
{
    public interface IMergableBlock : ISpriteSettable, ITypedBlock
    {
        bool MergeUp { get; }
        bool MergeDown { get; }
        bool MergeLeft { get; }
        bool MergeRight { get; }
        
        void SetMergeData(WorldDirections direction, bool value);
    }
}