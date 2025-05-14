using System;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.SaveLoad;
using System.Threading.Tasks;
using _Memoriam.Script.Audio;
using _Memoriam.Script.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    [Preserve]
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private SceneDataBase sceneData;
        public bool IsNewGame { get; private set; } = false;
        public bool NoSave { get; private set; } = false;

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
        }

        public async void NewGame()
        {
            IsNewGame = true;
            AudioManager.Instance.PlayMusic("GameplayMusic");
            await Loader.Instance.LoadLoader(true);
        }
                
        public async void LoadGame(int slot)
        {
            if (DataPersistentManager.Instance.FileDataHandler.DoesSaveExist(slot))
            {
                IsNewGame = false;
                NoSave = false;
                AudioManager.Instance.PlayMusic("GameplayMusic");
                await Loader.Instance.LoadLoader(true, slot); 
            }
            else
            {
                NoSave = true;
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