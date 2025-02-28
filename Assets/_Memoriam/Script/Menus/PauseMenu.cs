using _Memoriam.Script.Managers;
using UnityEngine;
using Zenject;

namespace _Memoriam.Script.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pauseMenu;
        [SerializeField] private SceneDataBase sceneData;
        
        public void Resume()
        {
            pauseMenu.SetActive(false);    
            GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnGameplay);
        }

        public async void Menu()
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneData.LoadingSceneName);
        }
    }
}
