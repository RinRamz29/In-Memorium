using System;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.SaveLoad;
using UnityEngine;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private SceneDataBase sceneData;
        [SerializeField] private GameObject errorPopUp;
        [SerializeField] private Button errorButton;
            
        private void OnEnable()
        {
            InputReader.Instance.PlayerActions.UI.Enable();

            InputReader.Instance.OnControlTypeChanged += SwitchCursorMode;
            
            GameStateManager.Instance.OnGameStateChanged.Invoke(GameStateManager.GameState.OnMenu);
        }

        public async void NewGame()
        {
            DataPersistentManager.Instance.IsNewGame = true;
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }
        
        public async void LoadGame()
        {
            DataPersistentManager.Instance.IsNewGame = false;

            if (DataPersistentManager.Instance.GameData == null)
            {
                errorButton.Select();
                errorPopUp.SetActive(true);
                return;
            }
            
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
            InputReader.Instance.OnControlTypeChanged -= SwitchCursorMode;
            InputReader.Instance.PlayerActions.UI.Disable();
        }
    }
}