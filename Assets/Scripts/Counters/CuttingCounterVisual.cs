using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>Rocks the knife up and down while someone is chopping, with a chop sound each time it lands.</summary>
    public class CuttingCounterVisual : MonoBehaviour
    {
        [SerializeField] private CuttingCounter counter;
        [SerializeField] private Transform knifePivot;
        [SerializeField] private float chopsPerSecond = 6f;
        [SerializeField] private float chopAngle = 35f;
        [SerializeField] private AudioSource chopSound;

        private Quaternion restRotation;
        private int lastStroke;

        private void Awake()
        {
            restRotation = knifePivot.localRotation;
        }

        private void LateUpdate()
        {
            if (!counter.IsChopping)
            {
                knifePivot.localRotation = restRotation;
                return;
            }

            float t = Mathf.Abs(Mathf.Sin(Time.time * chopsPerSecond * Mathf.PI));
            knifePivot.localRotation = restRotation * Quaternion.Euler(chopAngle * t, 0f, 0f);

            // The blade touches the board once per stroke.
            int stroke = Mathf.FloorToInt(Time.time * chopsPerSecond);
            if (stroke != lastStroke)
            {
                lastStroke = stroke;
                if (chopSound != null)
                    chopSound.Play();
            }
        }
    }
}
