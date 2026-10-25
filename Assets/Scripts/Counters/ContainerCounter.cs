using System;
using NomadChef.KitchenObjects;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Ingredient crate. Empty-handed players grab a fresh ingredient; its top also works as a worktop.
    /// </summary>
    public class ContainerCounter : BaseCounter
    {
        [SerializeField] private KitchenObjectSO kitchenObjectSO;

        public event Action PlayerGrabbedObject;

        public KitchenObjectSO KitchenObjectSO => kitchenObjectSO;

        public override void Interact(PlayerController player)
        {
            if (!this.HasKitchenObject() && !player.HasKitchenObject())
            {
                KitchenObject.Spawn(kitchenObjectSO, player);
                PlayerGrabbedObject?.Invoke();
                return;
            }

            SwapWithPlayer(player);
        }
    }
}
