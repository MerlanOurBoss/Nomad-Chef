using System;
using NomadChef.KitchenObjects;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Rack of clean plates. The kitchen owns a fixed number of plates; served plates come back
    /// dirty and return here once washed. Empty-handed players take a plate; players holding a
    /// plate-able ingredient get a plate with that ingredient already on it.
    /// </summary>
    public class PlatesCounter : BaseCounter, IPlateStack
    {
        [SerializeField] private KitchenObjectSO plateSO;
        [SerializeField, Min(0)] private int startingPlates = 4;

        private int plateCount;

        public event Action<int> PlateCountChanged;

        public int PlateCount => plateCount;

        private void Start()
        {
            SetPlateCount(startingPlates);
        }

        public void AddPlate() => SetPlateCount(plateCount + 1);

        public override void Interact(PlayerController player)
        {
            if (plateCount == 0)
                return;

            if (!player.HasKitchenObject())
            {
                KitchenObject.Spawn(plateSO, player);
                SetPlateCount(plateCount - 1);
                return;
            }

            KitchenObjectSO held = player.KitchenObject.Data;
            if (player.KitchenObject.TryGetPlate(out _) || !(plateSO.Prefab is PlateKitchenObject platePrefab) || !platePrefab.CanAdd(held))
                return;

            player.KitchenObject.DestroySelf();
            var plate = (PlateKitchenObject)KitchenObject.Spawn(plateSO, player);
            plate.TryAddIngredient(held);
            SetPlateCount(plateCount - 1);
        }

        private void SetPlateCount(int count)
        {
            plateCount = count;
            PlateCountChanged?.Invoke(plateCount);
        }
    }
}
