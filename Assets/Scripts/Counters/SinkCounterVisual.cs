using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>Water in the basin while plates are soaking; the sponge scrubs (with sound) while someone washes.</summary>
    public class SinkCounterVisual : MonoBehaviour
    {
        [SerializeField] private SinkCounter sink;
        [SerializeField] private GameObject water;
        [SerializeField] private Transform sponge;
        [SerializeField] private float scrubsPerSecond = 3f;
        [SerializeField] private float scrubDistance = 0.12f;
        [SerializeField] private AudioSource scrubLoop;

        private Vector3 spongeRest;

        private void Awake()
        {
            spongeRest = sponge.localPosition;
        }

        private void OnEnable()
        {
            sink.PlateCountChanged += OnPlateCountChanged;
            OnPlateCountChanged(sink.PlateCount);
        }

        private void OnDisable()
        {
            sink.PlateCountChanged -= OnPlateCountChanged;
        }

        private void LateUpdate()
        {
            float offset = sink.IsWashing ? Mathf.Sin(Time.time * scrubsPerSecond * Mathf.PI * 2f) * scrubDistance : 0f;
            sponge.localPosition = spongeRest + Vector3.right * offset;

            if (scrubLoop == null)
                return;
            if (sink.IsWashing && !scrubLoop.isPlaying)
                scrubLoop.Play();
            else if (!sink.IsWashing && scrubLoop.isPlaying)
                scrubLoop.Stop();
        }

        private void OnPlateCountChanged(int count) => water.SetActive(count > 0);
    }
}
