using System;
using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.Scripting;

namespace _Memoriam.Script.Managers
{
    [Preserve]
    public class GameStateManager : Singleton<GameStateManager>
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

        protected override void Awake()
        {
            base.Awake();
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
