using System;
using System.Collections.Generic;
using _Memoriam.Script.Enemies.Bosses;
using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Enemies
{
    [Preserve]    
    public class EnemyManager : Singleton<EnemyManager>
    {
        [SerializeField] private List<EnemyToSpawn> enemiesToSpawn  = new List<EnemyToSpawn>();
        [SerializeField] private List<EnemyToSpawn> flyersToSpawn  = new List<EnemyToSpawn>();
        [SerializeField] private List<EnemyToSpawn> rangedEnemies  = new List<EnemyToSpawn>();
        [SerializeField] private EnemyToSpawn miniBoss = null;
        [SerializeField] public string idForBasicEnemies;
        [SerializeField] public string idForFlyerEnemies;
        [SerializeField] public string idForRangedEnemies;
        [SerializeField] public string idForMiniBoss;
        
        [ContextMenu("Generate GUID for id")]
        private void GenerateId()
        {
            foreach (var spawn in enemiesToSpawn) 
                spawn.id = Guid.NewGuid().ToString();
            foreach (var flyer in flyersToSpawn)
                flyer.id = Guid.NewGuid().ToString();
            foreach (var tank in rangedEnemies)
                tank.id = Guid.NewGuid().ToString();
            
            miniBoss.id = Guid.NewGuid().ToString();
        }

        public void SpawnEnemies(bool newGame)
        {
            foreach (var spawn in enemiesToSpawn)
            {
                var enemy = ObjectPool.Instance.SpawnFromPool(idForBasicEnemies, spawn.spawnPoint.position,
                    spawn.spawnPoint.rotation, newGame);

                if (enemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.OffsetPoints = spawn.path;
                    enemyBase.id = spawn.id;
                }
            }

            foreach (var flyers in flyersToSpawn)
            {
                var enemy = ObjectPool.Instance.SpawnFromPool(idForFlyerEnemies, flyers.spawnPoint.position,
                    flyers.spawnPoint.rotation, newGame);

                if (enemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.OffsetPoints = flyers.path;
                    enemyBase.id = flyers.id;
                }
            }
            
            foreach (var ranged in rangedEnemies)
            {
                var enemy = ObjectPool.Instance.SpawnFromPool(idForRangedEnemies, ranged.spawnPoint.position,
                    ranged.spawnPoint.rotation, newGame);

                if (enemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.OffsetPoints = ranged.path;
                    enemyBase.id = ranged.id;
                }
            }

            var enemyBoss = ObjectPool.Instance.SpawnFromPool(idForMiniBoss, miniBoss.spawnPoint.position, miniBoss.spawnPoint.rotation, newGame);

            if (enemyBoss.TryGetComponent<BossEnemy>(out var boss))
            {
                boss.id = miniBoss.id;
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
