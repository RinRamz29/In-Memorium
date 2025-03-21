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
        [SerializeField] private List<EnemyToSpawn> flyersToSpawn  = new List<EnemyToSpawn>();
        [SerializeField] private List<EnemyToSpawn> tankEnemies  = new List<EnemyToSpawn>();
        [SerializeField] public string idForBasicEnemies;
        [SerializeField] public string idForFlyerEnemies;
        [SerializeField] public string idForTanksEnemies;
        
        [ContextMenu("Generate GUID for id")]
        private void GenerateId()
        {
            foreach (var spawn in enemiesToSpawn) 
                spawn.id = Guid.NewGuid().ToString();
            foreach (var flyer in flyersToSpawn)
                flyer.id = Guid.NewGuid().ToString();
            foreach (var tank in tankEnemies)
                tank.id = Guid.NewGuid().ToString();
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
            
            foreach (var tanks in tankEnemies)
            {
                var enemy = ObjectPool.Instance.SpawnFromPool(idForTanksEnemies, tanks.spawnPoint.position,
                    tanks.spawnPoint.rotation, newGame);

                if (enemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.OffsetPoints = tanks.path;
                    enemyBase.id = tanks.id;
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
