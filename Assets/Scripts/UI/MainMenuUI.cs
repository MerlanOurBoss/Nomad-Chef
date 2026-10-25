using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NomadChef.UI
{
    /// <summary>Title screen. Works with mouse, keyboard and gamepad (Play is selected by default).</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private string gameSceneName = "Kitchen_Greybox";

        private void Start()
        {
            playButton.onClick.AddListener(Play);
            quitButton.onClick.AddListener(Quit);
            Select(playButton);
        }

        private void Update()
        {
            // A mouse click on empty space clears the selection; keep keyboard/gamepad navigation working.
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
                Select(playButton);
        }

        private void Play() => SceneManager.LoadScene(gameSceneName);

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void Select(Selectable selectable)
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }
    }
}
