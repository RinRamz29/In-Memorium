using System;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.Audio;
using _Memoriam.Script.Localization;
using _Memoriam.Script.Menus;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting;

namespace _Memoriam.Script.Managers
{
    [Preserve]
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private SceneDataBase sceneData;
        [SerializeField] private SettingsMenu settingsMenu;
        
        protected override void Awake()
        {
            base.Awake();
            InputReader.Instance.PlayerActions.UI.Enable();
            AudioManager.Instance.PlayMusic("MainMenuMusic");
        }

        private void OnEnable()
        {
            InputReader.Instance.OnControlTypeChanged += SwitchCursorMode;
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnMenu);
            LocalizationManager.Instance.ForceTranslate();
            settingsMenu.SetSettings();
        }

        public async void NewGame()
        {
            Loader.Instance.IsNewGame = true;
            AudioManager.Instance.PlayMusic("GameplayMusic");
            await Loader.Instance.LoadLoader(true);
        }
                
        public async void LoadGame(int slot)
        {
            if (DataPersistentManager.Instance.FileDataHandler.DoesSaveExist(slot))
            {
                Loader.Instance.IsNewGame = false;
                AudioManager.Instance.PlayMusic("GameplayMusic");
                await Loader.Instance.LoadLoader(true, slot); 
            }
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