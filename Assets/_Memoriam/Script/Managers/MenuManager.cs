using System;
using System.Threading.Tasks;
using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Memoriam.Script.Managers
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private SceneDataBase sceneData;
        [Inject] private PlayerActionsScript _playerActionsScript;

        private void OnEnable()
        {
            _playerActionsScript.UI.Enable();
            GameStateManager.Instance.OnGameStateChanged.Invoke(GameStateManager.GameState.OnMenu);
        }

        public async void LoadLoadingScene()
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void OnDisable()
        {
            _playerActionsScript.UI.Disable();
        }
    }
}