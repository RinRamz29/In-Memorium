using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    public class Loader : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private SceneDataBase sceneData;

        private void OnEnable()
        {
            Debug.Log(GameStateManager.Instance.GameCurrentState);
            switch (GameStateManager.Instance.GameCurrentState)
            {
                case GameStateManager.GameState.OnPause:
                    Debug.Log("Loading Menu");
                    LoadMenu();
                    break;
                case GameStateManager.GameState.OnMenu:
                    Debug.Log("Loading Game");
                    LoadGamePlay();
                    break;
            }
        }

        private async void LoadGamePlay()
        {
            await LoadSceneAsync(sceneData.GameSceneName, progress =>
            {
                slider.value = progress;
                if (progress >= 0.9f)
                    GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnGameplay);
            });
        }

        private async void LoadMenu()
        {
            await LoadSceneAsync(sceneData.MainMenuSceneName, progress =>
            {
                slider.value = progress;
                if (progress >= 0.9f)
                    GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnMenu);
            });
        }

        private async Task LoadSceneAsync(string sceneName, Action<float> onProgress)
        {
            var loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            if (loadOperation == null)
            {
                onProgress?.Invoke(0f);
                return;
            }

            while (!loadOperation.isDone)
            {
                onProgress?.Invoke(loadOperation.progress);
                await Task.Yield();
            }

            onProgress?.Invoke(1.0f);
        }
    }
}