using UnityEngine;

namespace _Memoriam.Script.Player
{
    public interface IState
    {
        void Enter();
        void Exit();
        void Tick();
        void LateTick();
    }

    public class StateMachineBase
    {
        private IState _currentState;

        public void ChangeState(IState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        public void Tick()
        {
            _currentState?.Tick();
        }

        public void FixedTick()
        {
            _currentState?.LateTick();
        }

        public void ForceTerminate()
        {
            Debug.Log("I was force terminated");
            _currentState?.Exit();
        }
    }
}