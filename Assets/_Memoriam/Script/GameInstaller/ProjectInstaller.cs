using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using UnityEngine;
using Zenject;

namespace _Memoriam.Script.GameInstaller
{
    public class ProjectInstaller : MonoInstaller<ProjectInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerActionsScript>().AsSingle();
            Container.Bind<InputManager>().AsSingle();
        }
    }
}