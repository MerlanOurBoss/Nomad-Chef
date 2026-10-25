using System;

namespace NomadChef
{
    /// <summary>Anything that shows a pile of plates: the clean rack, the dirty return, a carried stack, the sink.</summary>
    public interface IPlateStack
    {
        int PlateCount { get; }
        event Action<int> PlateCountChanged;
    }
}
