using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>
    /// Fire and broth while something is in the kazan, a red progress bar and a flashing
    /// warning sign once cooked food is about to burn.
    /// </summary>
    public class KazanCounterVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private KazanCounter kazan;
        [SerializeField] private GameObject fire;
        [SerializeField] private GameObject broth;
        [SerializeField] private Transform warning;
        [SerializeField] private Renderer progressFill;
        [SerializeField] private Color cookColor = new Color(0.45f, 0.85f, 0.30f);
        [SerializeField] private Color burnColor = new Color(0.90f, 0.20f, 0.15f);
        [SerializeField] private float flashesPerSecond = 3f;

        private MaterialPropertyBlock block;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            kazan.StateChanged += OnStateChanged;
            OnStateChanged(kazan.CurrentState);
        }

        private void OnDisable()
        {
            kazan.StateChanged -= OnStateChanged;
        }

        private void LateUpdate()
        {
            bool flashOn = Mathf.Repeat(Time.time * flashesPerSecond, 1f) < 0.5f;
            warning.gameObject.SetActive(kazan.IsAboutToBurn && flashOn);
        }

        private void OnStateChanged(KazanCounter.State state)
        {
            bool heating = state == KazanCounter.State.Cooking || state == KazanCounter.State.Cooked;
            fire.SetActive(heating);
            broth.SetActive(state != KazanCounter.State.Idle);

            progressFill.GetPropertyBlock(block);
            block.SetColor(BaseColorId, state == KazanCounter.State.Cooked ? burnColor : cookColor);
            progressFill.SetPropertyBlock(block);
        }
    }
}
