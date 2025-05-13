using System;
using System.Threading.Tasks;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
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

        public async Task LoadLoader(bool loadGame, int slot = 0)
        {
            if (_isLoading) 
                return;
            
            _isLoading = true;
            
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
                await Task.Yield();
        }

        private async Task LoadGamePlay(int slot)
        {
            await LoadSceneAsync(sceneData.GameSceneName);

            if (MenuManager.Instance.IsNewGame)
            {
                ObjectPool.Instance.Initialize();
                EnemyManager.Instance.SpawnEnemies(true);
                PlayerSpawner.Instance.SpawnPlayer(true);
                TutorialManager.Instance.ResetTutorial();
                DataPersistentManager.Instance.NewGame();
            }
            else
            {
                ObjectPool.Instance.Initialize();
                EnemyManager.Instance.SpawnEnemies(false);
                PlayerSpawner.Instance.SpawnPlayer(true);
                DataPersistentManager.Instance.LoadGame(slot);
            }

            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnGameplay);
        }

        private async Task LoadMenu()
        {
            SceneCleanupUtility.CleanupScene();
            ObjectPool.Instance.ResetAllPools();
            await LoadSceneAsync(sceneData.MainMenuSceneName);
    
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnMenu);
        }

    }
}