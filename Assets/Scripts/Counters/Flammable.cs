using System;
using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// A counter that can catch fire. A burning counter cannot be used, slowly spreads the fire to
    /// a neighbouring counter, and regrows if the extinguisher stops spraying before it is out.
    /// </summary>
    public class Flammable : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float spreadDelay = 7f;
        [Tooltip("Seconds of continuous spraying needed to put the fire out.")]
        [SerializeField, Min(0.1f)] private float extinguishTime = 1.2f;
        [Tooltip("Fire regrows this fast (per second) when nobody is spraying.")]
        [SerializeField, Min(0f)] private float regrowRate = 0.5f;
        [SerializeField, Min(0.1f)] private float neighbourRadius = 0.6f;
        [SerializeField] private LayerMask counterLayerMask;

        private static readonly Collider[] Neighbours = new Collider[8];

        private bool onFire;
        private float fireStrength;
        private float spreadTimer;
        private float lastSprayTime = -1f;

        public event Action<bool> FireChanged;

        public bool IsOnFire => onFire;
        public float FireStrength => fireStrength;

        private void Update()
        {
            if (!onFire)
                return;

            if (Time.time - lastSprayTime > 0.2f)
                fireStrength = Mathf.Min(1f, fireStrength + regrowRate * Time.deltaTime);

            spreadTimer += Time.deltaTime;
            if (spreadTimer >= spreadDelay)
            {
                spreadTimer = 0f;
                SpreadToNeighbour();
            }
        }

        public void Ignite()
        {
            if (onFire)
                return;

            onFire = true;
            fireStrength = 1f;
            spreadTimer = 0f;
            FireChanged?.Invoke(true);
        }

        /// <summary>Called every frame the extinguisher hits this counter.</summary>
        public void Extinguish(float deltaTime)
        {
            if (!onFire)
                return;

            lastSprayTime = Time.time;
            fireStrength -= deltaTime / extinguishTime;
            if (fireStrength <= 0f)
            {
                onFire = false;
                fireStrength = 0f;
                FireChanged?.Invoke(false);
            }
        }

        private void SpreadToNeighbour()
        {
            Vector3 center = transform.position + Vector3.up * 0.5f;
            int count = Physics.OverlapSphereNonAlloc(center, neighbourRadius, Neighbours, counterLayerMask, QueryTriggerInteraction.Ignore);

            Flammable target = null;
            int candidates = 0;
            for (int i = 0; i < count; i++)
            {
                Flammable other = Neighbours[i].GetComponentInParent<Flammable>();
                if (other == null || other == this || other.onFire)
                    continue;

                // Reservoir sampling: pick a random unburnt neighbour without allocating a list.
                candidates++;
                if (UnityEngine.Random.Range(0, candidates) == 0)
                    target = other;
            }

            if (target != null)
                target.Ignite();
        }
    }
}
