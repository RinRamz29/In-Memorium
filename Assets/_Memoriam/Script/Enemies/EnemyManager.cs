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
                    enemyBase.OffsetPoints = spawn.path;
                    enemyBase.id = spawn.id;
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
                    enemyBase.OffsetPoints = flyers.path;
                    enemyBase.id = flyers.id;
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
                    enemyBase.OffsetPoints = ranged.path;
                    enemyBase.id = ranged.id;
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
                    bossEnemy.id = boss.id;
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