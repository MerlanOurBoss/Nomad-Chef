using System;
using NomadChef.Counters;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>
    /// Fire extinguisher tool. While a chef holds it and keeps "use" pressed, it sprays a cone in
    /// front of them that puts out burning counters.
    /// </summary>
    public class Extinguisher : KitchenObject
    {
        [SerializeField, Min(0.5f)] private float range = 2.2f;
        [SerializeField, Range(5f, 90f)] private float coneHalfAngle = 35f;
        [SerializeField] private LayerMask counterLayerMask;
        [SerializeField] private ParticleSystem spray;
        [SerializeField] private AudioSource hiss;

        private static readonly Collider[] Hits = new Collider[16];

        private bool spraying;

        public event Action<bool> SprayingChanged;

        public bool IsSpraying => spraying;

        public void SetSpraying(bool on)
        {
            if (on == spraying)
                return;

            spraying = on;
            if (spraying)
            {
                spray.Play();
                if (hiss != null)
                    hiss.Play();
            }
            else
            {
                spray.Stop();
                if (hiss != null)
                    hiss.Stop();
            }
            SprayingChanged?.Invoke(spraying);
        }

        private void Update()
        {
            // Put down mid-spray: stop.
            if (!(Parent is PlayerController))
                SetSpraying(false);

            if (spraying)
                ExtinguishInCone();
        }

        private void ExtinguishInCone()
        {
            Vector3 origin = transform.position;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            int count = Physics.OverlapSphereNonAlloc(origin, range, Hits, counterLayerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Flammable flammable = Hits[i].GetComponentInParent<Flammable>();
                if (flammable == null || !flammable.IsOnFire)
                    continue;

                Vector3 toTarget = Vector3.ProjectOnPlane(flammable.transform.position - origin, Vector3.up);
                if (toTarget.magnitude <= range && Vector3.Angle(forward, toTarget) <= coneHalfAngle)
                    flammable.Extinguish(Time.deltaTime);
            }
        }
    }
}
