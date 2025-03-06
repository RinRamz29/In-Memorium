using System;
using _Memoriam.Script.General;
using UnityEngine;

namespace _Memoriam.Script.Managers
{
    public class GameStateManager : MonoSingleton<GameStateManager>
    {
        [Serializable]
        public enum GameState
        {
            OnGameplay,
            OnMenu,
            OnPause,
            OnLose,
            OnLoading,
        } 
        [field: SerializeField] public GameState GameCurrentState { get; private set; }
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
            Debug.Log("Game state changed to: " + GameCurrentState);
        }
    }
}
