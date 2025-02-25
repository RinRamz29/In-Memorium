using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using UnityEngine;
using Zenject;

namespace _Memoriam.Script.GameInstaller
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        [SerializeField] private InputManager inputManager;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private GameManager gameManager;
        
        public override void InstallBindings()
        {
            var playerActions = new PlayerActionsScript();
            playerActions.Enable();
            
            Container.Bind<PlayerActionsScript>().FromInstance(playerActions).AsSingle();
            Container.Bind<InputManager>().FromInstance(inputManager).AsSingle();
            Container.Bind<GameStateManager>().FromInstance(gameStateManager).AsSingle();
            Container.Bind<GameManager>().FromInstance(gameManager).AsSingle();
        }
    }
}