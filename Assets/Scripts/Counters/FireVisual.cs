using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>Flames (and their crackle) on a burning counter, shrinking as the fire is put out.</summary>
    public class FireVisual : MonoBehaviour
    {
        [SerializeField] private Flammable flammable;
        [SerializeField] private ParticleSystem flames;
        [SerializeField] private AudioSource crackle;

        private float baseEmission;

        private void Awake()
        {
            baseEmission = flames.emission.rateOverTimeMultiplier;
        }

        private void OnEnable()
        {
            flammable.FireChanged += SetBurning;
            SetBurning(flammable.IsOnFire);
        }

        private void OnDisable()
        {
            flammable.FireChanged -= SetBurning;
        }

        private void Update()
        {
            if (!flammable.IsOnFire)
                return;

            ParticleSystem.EmissionModule emission = flames.emission;
            emission.rateOverTimeMultiplier = baseEmission * Mathf.Lerp(0.25f, 1f, flammable.FireStrength);
        }

        private void SetBurning(bool burning)
        {
            if (burning)
            {
                flames.Play();
                if (crackle != null)
                    crackle.Play();
            }
            else
            {
                flames.Stop();
                if (crackle != null)
                    crackle.Stop();
            }
        }
    }
}
