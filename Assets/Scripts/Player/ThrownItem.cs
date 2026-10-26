using System;
using NomadChef.Counters;
using NomadChef.KitchenObjects;
using UnityEngine;

namespace NomadChef.Player
{
    /// <summary>
    /// An ingredient flying through the air. It lands on the first thing it hits: an empty-handed
    /// chef catches it, a counter takes it if it would accept it, anything else (the floor, a full
    /// counter, the gap between caravan carts) means it is lost.
    /// </summary>
    public class ThrownItem : MonoBehaviour
    {
        private const float Gravity = -18f;
        private const float Radius = 0.2f;
        private const float GroundHeight = 0.05f;
        private const float MaxFlightTime = 3f;

        private static readonly RaycastHit[] Hits = new RaycastHit[8];

        private KitchenObject item;
        private PlayerController thrower;
        private Vector3 velocity;
        private LayerMask hitMask;
        private float age;

        /// <summary>Raised when a chef throws something (drives the throw animation and whoosh).</summary>
        public static event Action<PlayerController> AnyLaunched;

        // Static events survive play sessions when domain reload is disabled, so clear them on start.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents() => AnyLaunched = null;

        public static void Launch(KitchenObject item, PlayerController thrower, Vector3 velocity, LayerMask hitMask)
        {
            AnyLaunched?.Invoke(thrower);
            item.Detach();
            var flight = item.gameObject.AddComponent<ThrownItem>();
            flight.item = item;
            flight.thrower = thrower;
            flight.velocity = velocity;
            flight.hitMask = hitMask;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            velocity.y += Gravity * dt;

            Vector3 start = transform.position;
            Vector3 step = velocity * dt;
            if (TryLand(start, step))
                return;

            transform.position = start + step;
            if (transform.position.y < GroundHeight || age > MaxFlightTime)
                Lose();
        }

        private bool TryLand(Vector3 start, Vector3 step)
        {
            int count = Physics.SphereCastNonAlloc(start, Radius, step.normalized, Hits, step.magnitude, hitMask,
                QueryTriggerInteraction.Ignore);

            // Results are unordered: handle the closest valid target first.
            RaycastHit? nearest = null;
            for (int i = 0; i < count; i++)
            {
                if (Hits[i].collider.GetComponentInParent<PlayerController>() == thrower)
                    continue;
                if (nearest == null || Hits[i].distance < nearest.Value.distance)
                    nearest = Hits[i];
            }
            if (nearest == null)
                return false;

            Collider hit = nearest.Value.collider;
            enabled = false;
            Destroy(this);

            var catcher = hit.GetComponentInParent<PlayerController>();
            if (catcher != null)
            {
                if (catcher.HasKitchenObject())
                    Lose();
                else
                    item.SetParent(catcher);
                return true;
            }

            var counter = hit.GetComponentInParent<BaseCounter>();
            if (counter == null || !counter.TryCatch(item))
                Lose();
            return true;
        }

        private void Lose()
        {
            enabled = false;
            item.DestroySelf();
        }
    }
}
