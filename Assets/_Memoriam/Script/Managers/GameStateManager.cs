using System;
using UnityEngine;
using Zenject;

namespace _Memoriam.Script.Managers
{
    public class GameStateManager : MonoBehaviour
    {
        #region Internal
        [Serializable]
        public enum GameState
        {
            OnGameplay,
            OnPause,
            OnLose,
        } 
        public GameState GameCurrentState { get; set; }
        public Action<GameState> OnGameStateChanged { get; set; }

        private void OnEnable()
        {
            OnGameStateChanged += ChangeState;
        }

        private void OnDisable()
        {
            OnGameStateChanged -= ChangeState;
        }

        private void ChangeState(GameState newState)
        {
            GameCurrentState = newState;

            switch (GameCurrentState)
            {
                case GameState.OnGameplay:
                    GameplayLogic();
                    break;
                case GameState.OnPause:
                    PauseLogic();
                    break;
                case GameState.OnLose:
                    LoseLogic();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        [Inject] private PlayerActionsScript _playerActionsScript;
        
        private void PauseLogic()
        {
            _playerActionsScript.Player.Disable();
            _playerActionsScript.UI.Enable();
        }

        private void GameplayLogic()
        {
            _playerActionsScript.Player.Enable();
            _playerActionsScript.UI.Disable();
        }

        private void LoseLogic()
        {
            
        }
    }
}
