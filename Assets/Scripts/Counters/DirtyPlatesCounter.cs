using System;
using System.Collections.Generic;
using NomadChef.KitchenObjects;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Where served plates come back dirty, a few seconds after each delivery.
    /// Players pick up the whole pile at once.
    /// </summary>
    public class DirtyPlatesCounter : BaseCounter, IPlateStack
    {
        [SerializeField] private DeliveryCounter deliveryCounter;
        [SerializeField] private KitchenObjectSO dirtyPlatesSO;
        [SerializeField, Min(0f)] private float returnDelay = 6f;

        private readonly List<float> pendingReturns = new List<float>();
        private int plateCount;

        public event Action<int> PlateCountChanged;

        public int PlateCount => plateCount;

        private void OnEnable()
        {
            deliveryCounter.DeliverySucceeded += ScheduleReturn;
        }

        private void OnDisable()
        {
            deliveryCounter.DeliverySucceeded -= ScheduleReturn;
        }

        private void Update()
        {
            for (int i = pendingReturns.Count - 1; i >= 0; i--)
            {
                pendingReturns[i] -= Time.deltaTime;
                if (pendingReturns[i] <= 0f)
                {
                    pendingReturns.RemoveAt(i);
                    SetPlateCount(plateCount + 1);
                }
            }
        }

        public override void Interact(PlayerController player)
        {
            if (plateCount == 0)
                return;

            if (!player.HasKitchenObject())
            {
                var pile = (DirtyPlatesKitchenObject)KitchenObject.Spawn(dirtyPlatesSO, player);
                pile.AddPlates(plateCount);
                SetPlateCount(0);
            }
            else if (player.KitchenObject is DirtyPlatesKitchenObject heldPile)
            {
                heldPile.AddPlates(plateCount);
                SetPlateCount(0);
            }
        }

        private void ScheduleReturn() => pendingReturns.Add(returnDelay);

        private void SetPlateCount(int count)
        {
            plateCount = count;
            PlateCountChanged?.Invoke(plateCount);
        }
    }
}
