using System;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Memoriam.Script.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pause;
        public GameObject pauseMenu;
        
        [SerializeField] private SceneDataBase sceneData;
        
        public void Resume()
        {
                pauseMenu.SetActive(false);
                pause.SetActive(false);
                GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnGameplay);
            
        }

        public void Pause(InputAction.CallbackContext context)
        {
                
                pauseMenu.SetActive(true);
                pause.SetActive(true);
                GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnPause);
            
        }

        public async void Menu()
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }
        

        private void OnEnable()
        {
            InputReader.Instance.PlayerActions.Player.Pause.performed += Pause;
        }

        private void OnDisable()
        {
            InputReader.Instance.PlayerActions.Player.Pause.performed -= Pause;
        }
    }
}
