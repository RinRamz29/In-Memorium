using System;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.General;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.Tutorial;
using UnityEngine;

namespace _Memoriam.Script.Managers
{
    public class GameLoader : MonoBehaviour
    {
        public static int slotIndex = 0;
        public static bool newGame = false;
        
        private void OnEnable()
        {
            switch (newGame)
            {
                case true:
                    EnemyManager.Instance.SpawnEnemies(true);
                    DataPersistentManager.Instance.NewGame();
                    break;
                case false:
                    EnemyManager.Instance.SpawnEnemies(false);
                    DataPersistentManager.Instance.LoadGame(slotIndex);
                    break;
            }
        }
    }
}
