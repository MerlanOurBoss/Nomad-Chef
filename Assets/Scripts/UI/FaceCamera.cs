using UnityEngine;

namespace NomadChef.UI
{
    /// <summary>Keeps world-space UI (bars, icons, warnings) turned toward the main camera.</summary>
    public class FaceCamera : MonoBehaviour
    {
        private Camera mainCamera;

        private void LateUpdate()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            if (mainCamera != null)
                transform.rotation = mainCamera.transform.rotation;
        }
    }
}
