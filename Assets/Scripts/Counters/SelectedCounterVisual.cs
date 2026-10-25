using UnityEngine;

namespace NomadChef.Counters
{
    /// <summary>Shows a highlight while at least one player is facing the counter.</summary>
    public class SelectedCounterVisual : MonoBehaviour
    {
        [SerializeField] private BaseCounter counter;
        [SerializeField] private GameObject[] visuals;

        private void OnEnable()
        {
            counter.SelectedChanged += SetVisible;
            SetVisible(counter.IsSelected);
        }

        private void OnDisable()
        {
            counter.SelectedChanged -= SetVisible;
        }

        private void SetVisible(bool visible)
        {
            foreach (GameObject visual in visuals)
                visual.SetActive(visible);
        }
    }
}
