using System;
using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>
    /// An item in the world. It always belongs to exactly one IKitchenObjectParent and follows its follow point.
    /// </summary>
    public class KitchenObject : MonoBehaviour
    {
        [SerializeField] private KitchenObjectSO data;

        private IKitchenObjectParent parent;

        /// <summary>Raised whenever any item changes hands. The bool is true when the item was just spawned.</summary>
        public static event Action<KitchenObject, IKitchenObjectParent, bool> AnyParentChanged;

        public KitchenObjectSO Data => data;
        public IKitchenObjectParent Parent => parent;

        /// <summary>
        /// Seconds this food has spent in its current kazan stage (cooking while raw, burning once cooked),
        /// so taking it out and putting it back doesn't reset the clock.
        /// </summary>
        public float HeatTime { get; set; }

        // Static events survive play sessions when domain reload is disabled, so clear them on start.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents() => AnyParentChanged = null;

        public static KitchenObject Spawn(KitchenObjectSO data, IKitchenObjectParent parent)
        {
            KitchenObject kitchenObject = Instantiate(data.Prefab);
            kitchenObject.SetParent(parent);
            return kitchenObject;
        }

        public void SetParent(IKitchenObjectParent newParent)
        {
            if (newParent.HasKitchenObject())
            {
                Debug.LogError($"{newParent} already holds {newParent.KitchenObject.name}", this);
                return;
            }

            bool spawned = parent == null;
            if (parent != null)
                parent.KitchenObject = null;

            parent = newParent;
            parent.KitchenObject = this;

            transform.SetParent(parent.KitchenObjectFollowPoint, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            AnyParentChanged?.Invoke(this, parent, spawned);
        }

        public bool TryGetPlate(out PlateKitchenObject plate)
        {
            plate = this as PlateKitchenObject;
            return plate != null;
        }

        public void DestroySelf()
        {
            if (parent != null)
                parent.KitchenObject = null;

            Destroy(gameObject);
        }
    }
}
