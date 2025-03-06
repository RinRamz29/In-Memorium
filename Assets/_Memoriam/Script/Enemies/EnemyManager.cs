using System;
using System.Collections.Generic;
using System.Linq;
using _Memoriam.Script.General;
using NUnit.Framework;
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

        [Serializable]
        public class EnemyToSpawn
        {
            public List<Vector2> path;
            public Transform spawnPoint;
        }
        
        [SerializeField] private List<EnemyToSpawn> enemiesToSpawn = new List<EnemyToSpawn>();
        [SerializeField] public string idForEnemyPool;
        [SerializeField] private List<EnemySpawned> enemiesSpawned = new List<EnemySpawned>();
        
        private void Start()
        {
            foreach (var spawn in enemiesToSpawn)
            {
                var enemy = ObjectPool.Instance.SpawnFromPool(idForEnemyPool, spawn.spawnPoint.position,
                    spawn.spawnPoint.rotation);
                
                var newEnemy = new EnemySpawned
                {
                    isActive = true,
                    enemyObject = enemy
                };
                enemiesSpawned.Add(newEnemy);
                
                if (enemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.OffsetPoints = spawn.path;                        
                }
            }
        }
    }
}