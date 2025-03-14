using System;
using System.Collections.Generic;
using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.Scripting;

namespace _Memoriam.Script.Enemies
{
    [Preserve]    
    public class EnemyManager : Singleton<EnemyManager>
    {
        [SerializeField] private List<EnemyToSpawn> enemiesToSpawn  = new List<EnemyToSpawn>();
        [SerializeField] public string idForEnemyPool;
        
        [ContextMenu("Generate GUID for id")]
        private void GenerateId()
        {
            foreach (var spawn in enemiesToSpawn) 
                spawn.id = Guid.NewGuid().ToString();
        }

        public void SpawnEnemies(bool newGame)
        {
            foreach (var spawn in enemiesToSpawn)
            {
                var enemy = ObjectPool.Instance.SpawnFromPool(idForEnemyPool, spawn.spawnPoint.position,
                    spawn.spawnPoint.rotation, newGame);

                if (enemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.OffsetPoints = spawn.path;
                    enemyBase.id = spawn.id;
                }
            }
        }
    }

    [Serializable]
    public class EnemyToSpawn
    {
        public List<Vector2> path;
        public Transform spawnPoint;
        public string id;
    }
}
