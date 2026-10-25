using System;
using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>A carried pile of dirty plates. Taken from the return counter and emptied into the sink.</summary>
    public class DirtyPlatesKitchenObject : KitchenObject, IPlateStack
    {
        private int plateCount;

        public event Action<int> PlateCountChanged;

        public int PlateCount => plateCount;

        public void AddPlates(int count)
        {
            plateCount += Mathf.Max(0, count);
            PlateCountChanged?.Invoke(plateCount);
        }
    }
}
