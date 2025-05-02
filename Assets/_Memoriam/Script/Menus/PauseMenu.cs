using System;
using _Memoriam.Script.Audio;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _Memoriam.Script.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject firstButton;
        [SerializeField] private GameObject playerUI;
        [SerializeField] private SceneDataBase sceneData;

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(firstButton);
            playerUI.SetActive(false);
        }

        public void Resume()
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonSelectSFX");
            pauseMenu.SetActive(false);
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnGameplay);
            playerUI.SetActive(true);
        }

        public async void Menu()
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}