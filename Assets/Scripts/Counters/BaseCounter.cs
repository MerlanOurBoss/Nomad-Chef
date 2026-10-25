using System;
using NomadChef.KitchenObjects;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Base for every station on the kitchen grid. Holds one item on its top and reacts to player interactions.
    /// </summary>
    public abstract class BaseCounter : MonoBehaviour, IKitchenObjectParent
    {
        [SerializeField] private Transform counterTopPoint;

        // Several players can look at the same counter in co-op, so selection is reference counted.
        private int selectorCount;
        private Flammable flammable;

        public event Action<bool> SelectedChanged;

        public Transform KitchenObjectFollowPoint => counterTopPoint;
        public KitchenObject KitchenObject { get; set; }
        public bool IsSelected => selectorCount > 0;

        /// <summary>Burning counters cannot be used until the fire is put out.</summary>
        public bool IsOnFire => flammable != null && flammable.IsOnFire;

        protected Flammable Flammable => flammable;

        protected virtual void Awake()
        {
            flammable = GetComponent<Flammable>();
        }

        public abstract void Interact(PlayerController player);

        public virtual void InteractAlternate(PlayerController player) { }

        public void AddSelector()
        {
            selectorCount++;
            if (selectorCount == 1)
                SelectedChanged?.Invoke(true);
        }

        public void RemoveSelector()
        {
            if (selectorCount == 0)
                return;

            selectorCount--;
            if (selectorCount == 0)
                SelectedChanged?.Invoke(false);
        }

        /// <summary>
        /// Default "put down / pick up" behaviour shared by most counters.
        /// When both hands are full and one item is a plate, the other item goes onto the plate.
        /// </summary>
        protected void SwapWithPlayer(PlayerController player)
        {
            if (!this.HasKitchenObject())
            {
                if (player.HasKitchenObject())
                    player.KitchenObject.SetParent(this);
            }
            else if (!player.HasKitchenObject())
            {
                KitchenObject.SetParent(player);
            }
            else
            {
                TryCombineOnPlate(player.KitchenObject, KitchenObject);
            }
        }

        /// <summary>Puts one of the two items onto the other if that one is a plate that accepts it.</summary>
        protected static bool TryCombineOnPlate(KitchenObject a, KitchenObject b)
        {
            if (a.TryGetPlate(out PlateKitchenObject plate) && plate.TryAddIngredient(b.Data))
            {
                b.DestroySelf();
                return true;
            }

            if (b.TryGetPlate(out plate) && plate.TryAddIngredient(a.Data))
            {
                a.DestroySelf();
                return true;
            }

            return false;
        }
    }
}
