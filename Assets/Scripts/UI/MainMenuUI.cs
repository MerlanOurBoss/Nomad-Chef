using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NomadChef.UI
{
    /// <summary>
    /// Title screen: Play and Quit. Play opens the level list, one button per level showing its
    /// best stars, plus Back. Works with mouse, keyboard and gamepad.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Serializable]
        private struct LevelButton
        {
            public Button button;
            public TMP_Text label;
            public string title;
            public string sceneName;
        }

        [Header("Main")]
        [SerializeField] private GameObject mainButtons;
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        [Header("Levels")]
        [SerializeField] private GameObject levelButtons;
        [SerializeField] private LevelButton[] levels;
        [SerializeField] private Button backButton;

        private void Start()
        {
            foreach (LevelButton level in levels)
            {
                string sceneName = level.sceneName;
                level.button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
                level.label.text = $"{level.title}   {GameManager.GetBestStars(sceneName)}/3 stars";
            }

            playButton.onClick.AddListener(() => ShowLevels(true));
            backButton.onClick.AddListener(() => ShowLevels(false));
            quitButton.onClick.AddListener(Quit);
            ShowLevels(false);
        }

        private void Update()
        {
            // A mouse click on empty space clears the selection; keep keyboard/gamepad navigation working.
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
                Select(FirstButton());
        }

        private void ShowLevels(bool show)
        {
            mainButtons.SetActive(!show);
            levelButtons.SetActive(show);
            Select(FirstButton());
        }

        private Button FirstButton() => levelButtons.activeSelf ? levels[0].button : playButton;

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
