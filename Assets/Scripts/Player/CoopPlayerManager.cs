using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace NomadChef.Player
{
    /// <summary>
    /// Local co-op joining. Player 1 starts on the left half of the keyboard.
    /// Enter joins a second player on the right half (arrows); any gamepad button joins a gamepad player.
    /// </summary>
    public class CoopPlayerManager : MonoBehaviour
    {
        public const string KeyboardLeftScheme = "KeyboardLeft";
        public const string KeyboardRightScheme = "KeyboardRight";
        public const string GamepadScheme = "Gamepad";

        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Color[] playerColors =
        {
            new Color(0.86f, 0.25f, 0.22f),
            new Color(0.22f, 0.48f, 0.86f),
            new Color(0.30f, 0.72f, 0.32f),
            new Color(0.95f, 0.75f, 0.20f),
        };
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private bool spawnKeyboardPlayerOnStart = true;

        private readonly List<PlayerController> players = new List<PlayerController>();
        private IDisposable anyButtonListener;

        public event Action<PlayerController> PlayerJoined;

        public IReadOnlyList<PlayerController> Players => players;

        private void OnEnable()
        {
            anyButtonListener = InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
        }

        private void OnDisable()
        {
            anyButtonListener?.Dispose();
            anyButtonListener = null;
        }

        private void Start()
        {
            if (spawnKeyboardPlayerOnStart && Keyboard.current != null)
                Join(KeyboardLeftScheme, Keyboard.current);
        }

        private void OnAnyButtonPress(InputControl control)
        {
            // Enter / Space / A also press pause-menu buttons; they must not join a player then.
            if (players.Count >= maxPlayers || (GameManager.Instance != null && GameManager.Instance.IsPaused))
                return;

            switch (control.device)
            {
                case Gamepad gamepad when !IsDevicePaired(gamepad):
                    Join(GamepadScheme, gamepad);
                    break;

                case Keyboard keyboard:
                    if ((control == keyboard.enterKey || control == keyboard.numpadEnterKey) && !IsSchemeUsed(KeyboardRightScheme))
                        Join(KeyboardRightScheme, keyboard);
                    else if (control == keyboard.spaceKey && !IsSchemeUsed(KeyboardLeftScheme))
                        Join(KeyboardLeftScheme, keyboard);
                    break;
            }
        }

        private void Join(string controlScheme, InputDevice device)
        {
            int index = players.Count;
            PlayerInput playerInput = PlayerInput.Instantiate(playerPrefab.gameObject, index,
                controlScheme: controlScheme, pairWithDevice: device);

            var player = playerInput.GetComponent<PlayerController>();
            player.name = $"Player {index + 1} ({controlScheme})";

            if (spawnPoints.Length > 0)
            {
                Transform spawn = spawnPoints[index % spawnPoints.Length];
                player.Teleport(spawn.position, spawn.rotation);
            }

            if (playerColors.Length > 0)
                player.SetColor(playerColors[index % playerColors.Length]);

            players.Add(player);
            PlayerJoined?.Invoke(player);
        }

        private bool IsSchemeUsed(string controlScheme)
        {
            foreach (PlayerController player in players)
            {
                if (player.GetComponent<PlayerInput>().currentControlScheme == controlScheme)
                    return true;
            }
            return false;
        }

        private bool IsDevicePaired(InputDevice device)
        {
            foreach (PlayerController player in players)
            {
                ReadOnlyArray<InputDevice> devices = player.GetComponent<PlayerInput>().devices;
                foreach (InputDevice paired in devices)
                {
                    if (paired == device)
                        return true;
                }
            }
            return false;
        }
    }
}
