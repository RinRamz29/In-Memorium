using System;
using System.Threading.Tasks;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.SaveLoad;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace _Memoriam.Script.Managers
{
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private SceneDataBase sceneData;
        [Inject] private PlayerActionsScript _playerActionsScript;
        
            
        private void OnEnable()
        {
            _playerActionsScript.UI.Enable();

            InputReader.Instance.OnControlTypeChanged += SwitchCursorMode;
            
            GameStateManager.Instance.OnGameStateChanged.Invoke(GameStateManager.GameState.OnMenu);
        }

        public async void NewGame()
        {
            DataPersistentManager.Instance.isNewGame = true;
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }
        
        public async void LoadGame()
        {
            DataPersistentManager.Instance.isNewGame = false;
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }

        private void SwitchCursorMode(InputReader.ControlType controlType) 
        {
            switch (controlType)
            {
                case InputReader.ControlType.Control:
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Confined;
                    break;
                case InputReader.ControlType.KeyboardMouse:
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDisable()
        {
            InputReader.Instance.OnControlTypeChanged += SwitchCursorMode;
            _playerActionsScript.UI.Disable();
        }
    }
}