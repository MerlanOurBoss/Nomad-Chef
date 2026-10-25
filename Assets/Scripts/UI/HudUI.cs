using TMPro;
using UnityEngine;

namespace NomadChef.UI
{
    /// <summary>Level timer and score in the corner of the screen.</summary>
    public class HudUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField, Min(0f)] private float hurryUpTime = 30f;
        [SerializeField] private Color normalTimerColor = Color.white;
        [SerializeField] private Color hurryTimerColor = new Color(1f, 0.35f, 0.25f);

        private int shownSeconds = -1;

        private void Start()
        {
            GameManager.Instance.ScoreChanged += SetScore;
            SetScore(GameManager.Instance.Score);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ScoreChanged -= SetScore;
        }

        private void Update()
        {
            float remaining = GameManager.Instance.LevelTimeRemaining;
            int seconds = Mathf.CeilToInt(remaining);
            if (seconds == shownSeconds)
                return;

            shownSeconds = seconds;
            timerText.text = $"{seconds / 60}:{seconds % 60:00}";
            timerText.color = remaining <= hurryUpTime ? hurryTimerColor : normalTimerColor;
        }

        private void SetScore(int score)
        {
            scoreText.text = $"Score {score}";
        }
    }
}
