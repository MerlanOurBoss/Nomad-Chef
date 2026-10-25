using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>Bubbling while the kazan is on the fire, warning beeps when the food is about to burn.</summary>
    public class KazanCounterSound : MonoBehaviour
    {
        [SerializeField] private KazanCounter kazan;
        [SerializeField] private AudioSource boilLoop;
        [SerializeField] private AudioSource warningBeep;
        [SerializeField, Min(0.05f)] private float beepInterval = 0.33f;

        private float beepTimer;

        private void OnEnable()
        {
            kazan.StateChanged += OnStateChanged;
            OnStateChanged(kazan.CurrentState);
        }

        private void OnDisable()
        {
            kazan.StateChanged -= OnStateChanged;
        }

        private void Update()
        {
            if (!kazan.IsAboutToBurn)
            {
                beepTimer = 0f;
                return;
            }

            beepTimer -= Time.deltaTime;
            if (beepTimer <= 0f)
            {
                beepTimer = beepInterval;
                warningBeep.Play();
            }
        }

        private void OnStateChanged(KazanCounter.State state)
        {
            bool heating = state == KazanCounter.State.Cooking || state == KazanCounter.State.Cooked;
            if (heating && !boilLoop.isPlaying)
                boilLoop.Play();
            else if (!heating)
                boilLoop.Stop();
        }
    }
}
