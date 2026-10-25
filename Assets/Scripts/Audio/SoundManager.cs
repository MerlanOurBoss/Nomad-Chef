using NomadChef.Counters;
using NomadChef.KitchenObjects;
using NomadChef.Orders;
using NomadChef.Player;
using UnityEngine;

namespace NomadChef.Audio
{
    /// <summary>
    /// Plays the one-shot game sounds: picking up / putting down, deliveries, orders, washed plates,
    /// the countdown and the final whistle. Looping station sounds live on the stations themselves.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private OrderManager orderManager;
        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

        [Header("Clips")]
        [SerializeField] private AudioClip pickup;
        [SerializeField] private AudioClip drop;
        [SerializeField] private AudioClip deliverySuccess;
        [SerializeField] private AudioClip deliveryFail;
        [SerializeField] private AudioClip orderNew;
        [SerializeField] private AudioClip orderExpired;
        [SerializeField] private AudioClip plateWashed;
        [SerializeField] private AudioClip countdownTick;
        [SerializeField] private AudioClip countdownGo;
        [SerializeField] private AudioClip timesUp;

        private DeliveryCounter[] deliveryCounters;
        private SinkCounter[] sinks;
        private int lastCountdownSecond = -1;

        private void Start()
        {
            KitchenObject.AnyParentChanged += OnItemMoved;
            orderManager.OrderAdded += OnOrderAdded;
            orderManager.OrderExpired += OnOrderExpired;

            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged += OnGameStateChanged;

            deliveryCounters = FindObjectsByType<DeliveryCounter>();
            foreach (DeliveryCounter counter in deliveryCounters)
            {
                counter.DeliverySucceeded += OnDeliverySucceeded;
                counter.DeliveryFailed += OnDeliveryFailed;
            }

            sinks = FindObjectsByType<SinkCounter>();
            foreach (SinkCounter sink in sinks)
                sink.PlateWashed += OnPlateWashed;
        }

        private void OnDestroy()
        {
            KitchenObject.AnyParentChanged -= OnItemMoved;
            orderManager.OrderAdded -= OnOrderAdded;
            orderManager.OrderExpired -= OnOrderExpired;

            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= OnGameStateChanged;

            if (deliveryCounters != null)
            {
                foreach (DeliveryCounter counter in deliveryCounters)
                {
                    counter.DeliverySucceeded -= OnDeliverySucceeded;
                    counter.DeliveryFailed -= OnDeliveryFailed;
                }
            }

            if (sinks != null)
            {
                foreach (SinkCounter sink in sinks)
                    sink.PlateWashed -= OnPlateWashed;
            }
        }

        private void Update()
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.CurrentState != GameManager.State.Countdown)
                return;

            int second = Mathf.CeilToInt(game.CountdownRemaining);
            if (second != lastCountdownSecond)
            {
                lastCountdownSecond = second;
                Play(countdownTick);
            }
        }

        private void OnItemMoved(KitchenObject item, IKitchenObjectParent newParent, bool spawned)
        {
            if (newParent is PlayerController)
                Play(pickup);
            else if (!spawned)
                Play(drop);
        }

        private void OnOrderAdded(Order order) => Play(orderNew);

        private void OnOrderExpired(Order order) => Play(orderExpired);

        private void OnDeliverySucceeded() => Play(deliverySuccess);

        private void OnDeliveryFailed() => Play(deliveryFail);

        private void OnPlateWashed() => Play(plateWashed);

        private void OnGameStateChanged(GameManager.State state)
        {
            if (state == GameManager.State.Playing)
                Play(countdownGo);
            else if (state == GameManager.State.GameOver)
                Play(timesUp);
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip, volume);
        }
    }
}
