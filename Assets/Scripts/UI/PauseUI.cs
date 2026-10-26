using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NomadChef.UI
{
    /// <summary>Pause panel with Resume, Restart and Main menu. Shown while the GameManager is paused.</summary>
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            GameManager game = GameManager.Instance;
            resumeButton.onClick.AddListener(() => game.SetPaused(false));
            restartButton.onClick.AddListener(game.Restart);
            mainMenuButton.onClick.AddListener(game.QuitToMenu);
            game.PausedChanged += OnPausedChanged;
            panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PausedChanged -= OnPausedChanged;
        }

        private void OnPausedChanged(bool paused)
        {
            panel.SetActive(paused);
            if (paused && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }
}
