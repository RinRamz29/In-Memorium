using System;
using System.Collections.Generic;
using System.Linq;
using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Enemies
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [Serializable]
        private class EnemySpawned
        {
            public GameObject enemyObject;
            public bool isActive;
        }
        
        [SerializeField] private List<Transform> enemySpawnPoints = new List<Transform>();
        [SerializeField] public string idForEnemyPool;
        [SerializeField] private List<EnemySpawned> enemiesSpawned = new List<EnemySpawned>();
        
        private void Start()
        {
            foreach (var spawn in enemySpawnPoints)
            {
                var newEnemy = new EnemySpawned
                {
                    isActive = true,
                    enemyObject = ObjectPool.Instance.SpawnFromPool(idForEnemyPool, spawn.position, spawn.rotation)
                };
                enemiesSpawned.Add(newEnemy);
            }
        }
    }
}