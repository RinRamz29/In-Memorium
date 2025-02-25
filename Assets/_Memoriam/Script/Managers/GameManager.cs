using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Memoriam.Script.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private PlayerActionsScript _playerActionsScript;

        private void OnEnable()
        {
            _playerActionsScript.Player.Pause.performed += OnPause;
        }

        public void OnPause(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            if (_gameStateManager.GameCurrentState == GameStateManager.GameState.OnPause)
            {
                _gameStateManager.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnGameplay);
            }
            else if (_gameStateManager.GameCurrentState == GameStateManager.GameState.OnGameplay)
            {
                _gameStateManager.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnPause);
            }
        }

        public void OnGameplay()
        {
            _gameStateManager.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnGameplay);
        }

        public void OnLose()
        {
            _gameStateManager.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnLose);
        }

        private void OnDisable()
        {
            _playerActionsScript.Player.Pause.performed -= OnPause;
        }
    }
}