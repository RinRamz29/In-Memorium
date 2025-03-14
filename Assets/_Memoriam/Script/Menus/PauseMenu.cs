using System;
using _Memoriam.Script.Managers;
using UnityEngine;

namespace _Memoriam.Script.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pause;
        public GameObject pauseMenu;
        public GameObject LoadMenu;
        [SerializeField] private SceneDataBase sceneData;
        
        public void Resume()
        {
                pauseMenu.SetActive(false);
                pause.SetActive(false);
                GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnGameplay);
            
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                pauseMenu.SetActive(true);
                pause.SetActive(true);
                GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnPause);
            }
        }

        public async void Menu()
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }

        public void LoadGame()
        {
            pause.SetActive(false);
            LoadMenu.SetActive(true);
        }

        public void pauseMenuLoad()
        {
            pause.SetActive(true);
            LoadMenu.SetActive(false);
        }
    }
}
