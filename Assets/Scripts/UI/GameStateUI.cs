using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NomadChef.UI
{
    /// <summary>Big "3, 2, 1, Go!" countdown and the end-of-level screen with score and stars.</summary>
    public class GameStateUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text countdownText;
        [SerializeField, Min(0f)] private float goDuration = 0.8f;

        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Color starOnColor = new Color(1f, 0.80f, 0.20f);
        [SerializeField] private Color starOffColor = new Color(0.35f, 0.35f, 0.38f);
        [SerializeField] private GameObject restartHint;

        private float goTimeLeft;

        private void Start()
        {
            GameManager.Instance.StateChanged += OnStateChanged;
            OnStateChanged(GameManager.Instance.CurrentState);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= OnStateChanged;
        }

        private void Update()
        {
            GameManager game = GameManager.Instance;

            if (game.CurrentState == GameManager.State.Countdown)
            {
                countdownText.text = Mathf.CeilToInt(game.CountdownRemaining).ToString();
            }
            else if (goTimeLeft > 0f)
            {
                goTimeLeft -= Time.deltaTime;
                if (goTimeLeft <= 0f)
                    countdownText.gameObject.SetActive(false);
            }

            restartHint.SetActive(game.CanRestart);
        }

        private void OnStateChanged(GameManager.State state)
        {
            countdownText.gameObject.SetActive(state != GameManager.State.GameOver);
            gameOverPanel.SetActive(state == GameManager.State.GameOver);

            if (state == GameManager.State.Playing)
            {
                countdownText.text = "Go!";
                goTimeLeft = goDuration;
            }
            else if (state == GameManager.State.GameOver)
            {
                ShowResults();
            }
        }

        private void ShowResults()
        {
            GameManager game = GameManager.Instance;
            finalScoreText.text = $"Score {game.Score}";
            for (int i = 0; i < starImages.Length; i++)
                starImages[i].color = i < game.Stars ? starOnColor : starOffColor;
        }
    }
}
