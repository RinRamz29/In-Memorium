namespace _Memoriam.Script.Player
{
    namespace VeilOfShadows.Hea.StateMachine
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
            private IState _previousState;
    
            public void ChangeState(IState newState)
            {
                _currentState?.Exit();
                _previousState = _currentState;
                _currentState = newState;
                _currentState.Enter();
            }
    
            public void Tick()
            {
                _currentState?.Tick();
            }
    
            public void LateTick()
            {
                _currentState?.LateTick();
            }
    
            public void RevertToPreviousState()
            {
                if (_previousState != null)
                {
                    ChangeState(_previousState);
                }
            }
        }
    }

}