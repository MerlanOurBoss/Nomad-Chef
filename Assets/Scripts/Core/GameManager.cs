using System;
using NomadChef.Orders;
using NomadChef.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NomadChef
{
    /// <summary>
    /// Runs one level: countdown, timed play with orders, then game over with score and stars.
    /// On the game over screen any player can press Interact to replay or Dash to go back to the menu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum State
        {
            Countdown,
            Playing,
            GameOver,
        }

        [SerializeField] private OrderManager orderManager;
        [SerializeField] private CoopPlayerManager playerManager;
        [SerializeField, Min(0f)] private float countdownDuration = 3f;
        [SerializeField, Min(1f)] private float levelDuration = 180f;
        [SerializeField, Min(0)] private int expiredOrderPenalty = 10;
        [Tooltip("Score needed for 1, 2 and 3 stars.")]
        [SerializeField] private int[] starThresholds = { 40, 100, 160 };
        [SerializeField, Min(0f)] private float restartDelay = 1.5f;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private State state;
        private float timer;
        private int score;

        public static GameManager Instance { get; private set; }

        public event Action<State> StateChanged;
        public event Action<int> ScoreChanged;

        public State CurrentState => state;
        public int Score => score;
        public bool CanPlayersAct => state == State.Playing;
        public bool CanRestart => state == State.GameOver && timer >= restartDelay;
        public float CountdownRemaining => state == State.Countdown ? timer : 0f;
        public float LevelTimeRemaining => state switch
        {
            State.Countdown => levelDuration,
            State.Playing => timer,
            _ => 0f,
        };
        public int MaxStars => starThresholds.Length;

        public int Stars
        {
            get
            {
                int stars = 0;
                foreach (int threshold in starThresholds)
                {
                    if (score >= threshold)
                        stars++;
                }
                return stars;
            }
        }

        private void Awake()
        {
            Instance = this;
            orderManager.enabled = false;
        }

        private void OnEnable()
        {
            orderManager.OrderCompleted += OnOrderCompleted;
            orderManager.OrderExpired += OnOrderExpired;
        }

        private void OnDisable()
        {
            orderManager.OrderCompleted -= OnOrderCompleted;
            orderManager.OrderExpired -= OnOrderExpired;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            SetState(State.Countdown);
        }

        private void Update()
        {
            switch (state)
            {
                case State.Countdown:
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                        SetState(State.Playing);
                    break;

                case State.Playing:
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                        SetState(State.GameOver);
                    break;

                case State.GameOver:
                    timer += Time.deltaTime;
                    if (!CanRestart)
                        break;
                    if (AnyPlayerPressed(p => p.InteractPressedThisFrame))
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    else if (AnyPlayerPressed(p => p.DashPressedThisFrame))
                        SceneManager.LoadScene(mainMenuSceneName);
                    break;
            }
        }

        private void SetState(State newState)
        {
            state = newState;
            timer = state switch
            {
                State.Countdown => countdownDuration,
                State.Playing => levelDuration,
                _ => 0f,
            };
            orderManager.enabled = state == State.Playing;
            StateChanged?.Invoke(state);
        }

        private void OnOrderCompleted(Order order, int points) => AddScore(points);

        private void OnOrderExpired(Order order) => AddScore(-expiredOrderPenalty);

        private void AddScore(int delta)
        {
            score = Mathf.Max(0, score + delta);
            ScoreChanged?.Invoke(score);
        }

        private bool AnyPlayerPressed(Func<PlayerController, bool> pressed)
        {
            foreach (PlayerController player in playerManager.Players)
            {
                if (pressed(player))
                    return true;
            }
            return false;
        }
    }
}
