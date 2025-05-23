using System;
using System.Linq;
using System.Threading.Tasks;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Localization;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    public class Loader : Singleton<Loader>
    {
        [SerializeField] private Slider slider;
        [SerializeField] private SceneDataBase sceneData;
        private bool _isLoading = false;
        public bool IsNewGame { get; set; }
        public bool SetTutorial { get; set; } = true;

        public async Task LoadLoader(bool loadGame, int slot = 0)
        {
            if (_isLoading)
                return;

            _isLoading = true;

            if (!loadGame)
            {
                await AwaitForCleanUp();
            }

            await LoadSceneAsync(sceneData.LoadingSceneName);

            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnLoading);
            InputReader.Instance.PlayerActions.Disable();

            slider = GameObject.FindWithTag("LoadingCanvas")?
                .GetComponentInChildren<Slider>();
            
            if (slider == null)
                Debug.LogWarning("[Loader] Slider no encontrado en LoadingScene.");
            
            if (loadGame)
                await LoadGamePlay(slot);
            else
                await LoadMenu();

            slider = null;
            _isLoading = false;
            InputReader.Instance.PlayerActions.Enable();
        }

        private async Task LoadSceneAsync(string sceneName)
        {
            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                await Task.Yield();
            }

            op.allowSceneActivation = true;
            while (!op.isDone)
            {
                await Task.Yield();
            }
        }

        private async Task LoadGamePlay(int slot)
        {
            await LoadSceneAsync(sceneData.GameSceneName);
            await WaitForLoadToComplete(slot);
        }

        private async Task WaitForLoadToComplete(int slot)
        {
            if (IsNewGame)
            {
                DataPersistentManager.Instance.NewGame();
                await ObjectPool.Instance.Initialize();
                await EnemySpawner.Instance.SpawnEnemies(true);
                PlayerSpawner.Instance.SpawnPlayer(true);
                TutorialManager.Instance.ResetTutorial(SetTutorial);
                LocalizationManager.Instance.ForceTranslate();
            }
            else
            {
                await ObjectPool.Instance.Initialize();
                await EnemySpawner.Instance.SpawnEnemies(false);
                PlayerSpawner.Instance.SpawnPlayer(false);
                DataPersistentManager.Instance.LoadGame(slot);
                LocalizationManager.Instance.ForceTranslate();
            }
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnGameplay);
            await GameplayMenuManager.Instance.Fade(0f, 2f);
        }
        
        private async Task LoadMenu()
        {
            await LoadSceneAsync(sceneData.MainMenuSceneName);

            LocalizationManager.Instance.ForceTranslate();
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnMenu);
        }

        private async Task AwaitForCleanUp()
        {
            await SceneCleanupUtility.Instance.CleanupScene();
            await ObjectPool.Instance.ResetAllPools();
        }
    }
}