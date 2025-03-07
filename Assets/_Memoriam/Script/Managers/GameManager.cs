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
            EnemyManager.Instance.SpawnEnemies(DataPersistentManager.Instance.isNewGame);
            
            switch (DataPersistentManager.Instance.isNewGame)
            {
                case true:
                    DataPersistentManager.Instance.NewGame();
                    break;
                case false:
                    DataPersistentManager.Instance.LoadGame();
                    break;
            }
        }
    }
}
