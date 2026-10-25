using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>Flashes the serving window green on a good delivery and red on a refused one.</summary>
    public class DeliveryCounterVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private DeliveryCounter counter;
        [SerializeField] private Renderer flashRenderer;
        [SerializeField] private Color successColor = new Color(0.35f, 0.85f, 0.35f);
        [SerializeField] private Color failColor = new Color(0.90f, 0.25f, 0.20f);
        [SerializeField, Min(0.05f)] private float flashDuration = 0.5f;

        private MaterialPropertyBlock block;
        private Color idleColor;
        private Color flashColor;
        private float flashTimeLeft;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            idleColor = flashRenderer.sharedMaterial.GetColor(BaseColorId);
        }

        private void OnEnable()
        {
            counter.DeliverySucceeded += FlashSuccess;
            counter.DeliveryFailed += FlashFail;
        }

        private void OnDisable()
        {
            counter.DeliverySucceeded -= FlashSuccess;
            counter.DeliveryFailed -= FlashFail;
        }

        private void Update()
        {
            if (flashTimeLeft <= 0f)
                return;

            flashTimeLeft -= Time.deltaTime;
            float t = Mathf.Clamp01(flashTimeLeft / flashDuration);
            SetColor(Color.Lerp(idleColor, flashColor, t));
        }

        private void FlashSuccess() => Flash(successColor);

        private void FlashFail() => Flash(failColor);

        private void Flash(Color color)
        {
            flashColor = color;
            flashTimeLeft = flashDuration;
            SetColor(color);
        }

        private void SetColor(Color color)
        {
            flashRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            flashRenderer.SetPropertyBlock(block);
        }
    }
}
