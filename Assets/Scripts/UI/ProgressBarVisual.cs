using UnityEngine;

namespace NomadChef.UI
{
    /// <summary>
    /// World-space progress bar. Listens to any IHasProgress on the target object
    /// and hides itself when progress is 0 or complete. Put a FaceCamera on the bar root.
    /// </summary>
    public class ProgressBarVisual : MonoBehaviour
    {
        [SerializeField] private GameObject hasProgressObject;
        [SerializeField] private GameObject barRoot;
        [SerializeField] private Transform fillPivot;

        private IHasProgress hasProgress;

        private void Awake()
        {
            hasProgress = hasProgressObject.GetComponent<IHasProgress>();
            if (hasProgress == null)
                Debug.LogError($"{hasProgressObject.name} has no IHasProgress component", this);
        }

        private void OnEnable()
        {
            if (hasProgress != null)
                hasProgress.ProgressChanged += SetProgress;
            SetProgress(0f);
        }

        private void OnDisable()
        {
            if (hasProgress != null)
                hasProgress.ProgressChanged -= SetProgress;
        }

        private void SetProgress(float progress)
        {
            fillPivot.localScale = new Vector3(progress, 1f, 1f);
            barRoot.SetActive(progress > 0f && progress < 1f);
        }
    }
}
