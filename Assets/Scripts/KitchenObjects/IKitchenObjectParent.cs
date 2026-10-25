using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>
    /// Anything that can hold exactly one KitchenObject: counters, players, later plates/pots.
    /// </summary>
    public interface IKitchenObjectParent
    {
        Transform KitchenObjectFollowPoint { get; }
        KitchenObject KitchenObject { get; set; }
    }

    public static class KitchenObjectParentExtensions
    {
        public static bool HasKitchenObject(this IKitchenObjectParent parent) => parent.KitchenObject != null;
    }
}
