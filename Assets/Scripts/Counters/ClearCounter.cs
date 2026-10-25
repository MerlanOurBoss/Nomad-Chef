using NomadChef.KitchenObjects;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>Plain worktop: put an item down or pick it up. Can start the level holding an item (e.g. the extinguisher).</summary>
    public class ClearCounter : BaseCounter
    {
        [SerializeField] private KitchenObjectSO startingKitchenObject;

        private void Start()
        {
            if (startingKitchenObject != null && !this.HasKitchenObject())
                KitchenObject.Spawn(startingKitchenObject, this);
        }

        public override void Interact(PlayerController player) => SwapWithPlayer(player);
    }
}
