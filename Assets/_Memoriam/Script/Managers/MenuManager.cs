using System;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.SaveLoad;
using TerrorConsole;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    [Preserve]
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private SceneDataBase sceneData;
        [SerializeField] private GameObject errorPopUp;
        [SerializeField] private Button errorButton;

        public void PlayButtonHoverSFX()
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonHoverSFX");
        }

        public void PlayButtonSelectSFX()
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonSelectSFX");
        }

        protected override void Awake()
        {
            base.Awake();
            InputReader.Instance.PlayerActions.UI.Enable();
        }

        private void OnEnable()
        {
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