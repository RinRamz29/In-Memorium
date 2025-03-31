using System;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.General;
using _Memoriam.Script.SaveLoad;
using TerrorConsole;
using UnityEngine;

namespace _Memoriam.Script.Managers
{
    public class GameLoader : MonoBehaviour
    {
        private void OnEnable()
        {
            switch (DataPersistentManager.Instance.IsNewGame)
            {
                case true:
                    EnemyManager.Instance.SpawnEnemies(true);
                    DataPersistentManager.Instance.NewGame();
                    break;
                case false:
                    EnemyManager.Instance.SpawnEnemies(false);
                    DataPersistentManager.Instance.LoadGame();
                    break;
            }
        }
    }
}
