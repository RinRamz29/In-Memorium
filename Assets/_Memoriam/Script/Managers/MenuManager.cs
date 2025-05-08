using System;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.SaveLoad;
using System.Threading.Tasks;
using _Memoriam.Script.Audio;
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
        [SerializeField] private GameObject firstToSelect;

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
            AudioManager.Instance.PlayMusic("MainMenuMusic");
        }

        private void OnEnable()
        {
            InputReader.Instance.OnControlTypeChanged += SwitchCursorMode;
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnMenu);
            EventSystem.current.SetSelectedGameObject(firstToSelect);
        }

        public async void NewGame()
        {
            await Task.Delay(150);
            AudioManager.Instance.PlayMusic("GameplayMusic");
            GameLoader.newGame = true;
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