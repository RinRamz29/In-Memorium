using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

namespace _Memoriam.Script.Managers
{
    public class GameplayMenuManager : MonoBehaviour
    {
        [Inject] private PlayerActionsScript _playerActions;

        [SerializeField] private GameObject pauseMenu;

        private void OnEnable()
        {
            _playerActions.Player.Pause.performed += OnPause;
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

        private void OnDisable()
        {
            _playerActions.Player.Pause.performed -= OnPause;
        }
    }
}