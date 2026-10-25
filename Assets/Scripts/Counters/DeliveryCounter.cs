using System;
using NomadChef.KitchenObjects;
using NomadChef.Orders;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Serving window. A plate that matches an open order is served and disappears;
    /// a plate nobody ordered is refused and stays in the player's hands.
    /// </summary>
    public class DeliveryCounter : BaseCounter
    {
        [SerializeField] private OrderManager orderManager;

        public event Action DeliverySucceeded;
        public event Action DeliveryFailed;

        public override void Interact(PlayerController player)
        {
            if (!player.HasKitchenObject() || !player.KitchenObject.TryGetPlate(out PlateKitchenObject plate))
                return;

            if (orderManager.TryDeliver(plate))
            {
                plate.DestroySelf();
                DeliverySucceeded?.Invoke();
            }
            else
            {
                DeliveryFailed?.Invoke();
            }
        }
    }
}
