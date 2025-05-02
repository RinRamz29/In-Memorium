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
        public event Action<GameState> OnGameStateChanged;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnGameStateChanged = null;
        }

        public void SetGameState(GameState newState)
        {
            if (GameCurrentState == newState)
                return;

            GameCurrentState = newState;
            OnGameStateChanged?.Invoke(GameCurrentState);
        }
    }
}
