using System;
using System.Collections.Generic;
using _Memoriam.Script.Enemies.MiniBoss;
using _Memoriam.Script.General;
using UnityEngine;

namespace _Memoriam.Script.Enemies
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [SerializeField] private List<EnemyToSpawn> enemiesToSpawn = new List<EnemyToSpawn>();
        [SerializeField] private List<EnemyToSpawn> flyersToSpawn = new List<EnemyToSpawn>();
        [SerializeField] private List<EnemyToSpawn> rangedEnemies = new List<EnemyToSpawn>();
        [SerializeField] private List<EnemyToSpawn> bosses = new List<EnemyToSpawn>();
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
            foreach (var boss in bosses)
                boss.id = Guid.NewGuid().ToString();
        }

        public void SpawnEnemies(bool newGame)
        {
            var counterBaseEnemy = 0;
            var counterFlyer = 0;
            var counterRanged = 0;
            var counterBoss = 0;

            foreach (var spawn in enemiesToSpawn)
            {
                counterBaseEnemy++;
                var spawnedEnemy = ObjectPool.Instance.GetReferenceFromPool(idForBasicEnemies, counterBaseEnemy,
                    spawn.spawnPoint.position,
                    spawn.spawnPoint.rotation, newGame);

                if (spawnedEnemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.PatrolPoints = spawn.path;
                    enemyBase.SaveLoad.id = spawn.id;
                    enemyBase.SpawnPosition = spawn.spawnPoint.position;
                }
            }

            foreach (var flyers in flyersToSpawn)
            {
                counterFlyer++;
                var spawnedEnemy = ObjectPool.Instance.GetReferenceFromPool(idForFlyerEnemies, counterFlyer,
                    flyers.spawnPoint.position,
                    flyers.spawnPoint.rotation, newGame);


                if (spawnedEnemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.PatrolPoints = flyers.path;
                    enemyBase.SaveLoad.id = flyers.id;
                    enemyBase.SpawnPosition = flyers.spawnPoint.position;
                }
            }

            foreach (var ranged in rangedEnemies)
            {
                counterRanged++;
                var spawnedEnemy = ObjectPool.Instance.GetReferenceFromPool(idForRangedEnemies, counterRanged,
                    ranged.spawnPoint.position,
                    ranged.spawnPoint.rotation, newGame);

                if (spawnedEnemy.TryGetComponent(out BaseEnemy enemyBase))
                {
                    enemyBase.PatrolPoints = ranged.path;
                    enemyBase.SaveLoad.id = ranged.id;
                    enemyBase.SpawnPosition = ranged.spawnPoint.position;
                }
            }

            foreach (var boss in bosses)
            {
                counterBoss++;
                var spawnedEnemy = ObjectPool.Instance.GetReferenceFromPool(idForMiniBoss, counterBoss,
                    boss.spawnPoint.position,
                    boss.spawnPoint.rotation, newGame);

                if (spawnedEnemy.TryGetComponent<BossEnemy>(out var bossEnemy))
                {
                    bossEnemy.SaveLoad.id  = boss.id;
                    bossEnemy.SpawnPosition = boss.spawnPoint.position;
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