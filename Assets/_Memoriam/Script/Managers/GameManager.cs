using System;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.General;
using _Memoriam.Script.SaveLoad;
using UnityEngine;

namespace _Memoriam.Script.Managers
{
    public class GameManager : MonoBehaviour
    {
        private void OnEnable()
        {
            
            switch (DataPersistentManager.Instance.isNewGame)
            {
                case true:
                    DataPersistentManager.Instance.NewGame();
                    EnemyManager.Instance.SpawnEnemies(true);
                    break;
                case false:
                    DataPersistentManager.Instance.LoadGame();
                    EnemyManager.Instance.SpawnEnemies(false);
                    break;
            }
            
        }
    }
}
