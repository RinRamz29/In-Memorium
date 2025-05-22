using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Memoriam.Script.General;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Enemies
{
    public class EnemySpawner : Singleton<EnemySpawner>
    {
        [SerializeField] private List<EnemyToSpawn> enemies;
        [SerializeField] private List<string> poolIdForEnemies;

        public async Task SpawnEnemies(bool newGame)
        {
            var counterForEnemies = 1;
            var counterForRanged = 1;
            var counterForBoss = 1;
            
            foreach (var spawn in enemies.Where(spawn => spawn.enemyType == EnemyType.Ghoul))
            {
                SpawnFromPool(spawn, "Enemies", newGame, counterForEnemies);
                counterForEnemies++;
                await Task.Yield();
            }
            
            foreach (var spawn in enemies.Where(spawn => spawn.enemyType == EnemyType.Ranged))
            {
                SpawnFromPool(spawn, "RangedEnemies", newGame, counterForRanged);
                counterForRanged++;
                await Task.Yield();
            }
            
            foreach (var spawn in enemies.Where(spawn => spawn.enemyType == EnemyType.MiniBoss))
            {
                SpawnFromPool(spawn, "Miniboss", newGame, counterForBoss);
                counterForBoss++;
                await Task.Yield();
            }
        }

        private void SpawnFromPool(EnemyToSpawn spawn, string id, bool newGame, int counter)
        {
            var spawnedEnemy = ObjectPool.Instance.GetReferenceFromPool(id, counter, spawn.spawnPoint.position, spawn.spawnPoint.rotation, newGame);
            
            if (spawnedEnemy.TryGetComponent(out BaseEnemy enemyBase))
            {
                enemyBase.PatrolPoints = spawn.path;
                enemyBase.SaveLoad.id = spawn.id;
                enemyBase.SpawnPosition = spawn.spawnPoint.position;
            }
        }

        #region Utils

        [ContextMenu("Generate GUID for Ghouls ID")]
        private void GenerateGhoulsId()
        {
            foreach (var ghouls in enemies)
            {
                if (ghouls.enemyType == EnemyType.Ghoul)
                    ghouls.id = Guid.NewGuid().ToString();
            }
        }

        [ContextMenu("Generate GUID for Ranged ID")]
        private void GenerateRangedId()
        {
            foreach (var ranged in enemies)
            {
                if (ranged.enemyType == EnemyType.Ranged)
                    ranged.id = Guid.NewGuid().ToString();
            }
        }

        [ContextMenu("Generate GUID for Bosses ID")]
        private void GenerateBossesId()
        {
            foreach (var boss in enemies)
            {
                if (boss.enemyType == EnemyType.MiniBoss)
                    boss.id = Guid.NewGuid().ToString();
            }
        }

        #endregion
    }

    [Serializable]
    public class EnemyToSpawn
    {
        public List<Vector2> path;
        public Transform spawnPoint;
        public EnemyType enemyType;
        public string id;
    }

    [Serializable]
    public enum EnemyType
    {
        Ghoul,
        Ranged,
        MiniBoss
    }
}