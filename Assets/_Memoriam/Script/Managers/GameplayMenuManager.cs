using System;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.SaveLoad;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    public class GameplayMenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenu;
        [field: SerializeField] public Slider HealthBar { get; private set; }

        private void ChangeHealthValue(float health) => HealthBar.value = health;

        private void OnEnable()
        {
            InputReader.Instance.PlayerActions.Player.Pause.performed += OnPause;
            Player.Player.OnHealthChanged += ChangeHealthValue;
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            switch (GameStateManager.Instance.GameCurrentState)
            {
                case GameStateManager.GameState.OnPause:
                    GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnGameplay);
                    pauseMenu.SetActive(false);
                    break;
                case GameStateManager.GameState.OnGameplay:
                    GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnPause);
                    pauseMenu.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Save()
        {
            DataPersistentManager.Instance.SaveGame();
        }

        private void OnDisable()
        {
            InputReader.Instance.PlayerActions.Player.Pause.performed -= OnPause;
            Player.Player.OnHealthChanged -= ChangeHealthValue;
        }
    }
}