using UnityEngine;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;

namespace _Memoriam.Script.Enemies.Components
{
    public class EnemySaveLoad : MonoBehaviour, ISaveableObject
    {
        [field: SerializeField] public string id { get; set; }
        private BaseEnemy _baseEnemy;

        public void LoadData(GameData data)
        {
            if (data.enemySavable.TryGetValue(id, out SavableEnemy enemyData))
            {
                gameObject.SetActive(enemyData.isAlive);
                transform.position = enemyData.position;
            }
        }

        public void SaveData(ref GameData data)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError($"Enemy {gameObject.name} has no ID assigned and cannot be saved.", this);
                return;
            }

            var instance = new SavableEnemy()
            {
                isAlive = gameObject.activeInHierarchy, 
                position = transform.position,
            };
            
            if (data.enemySavable.ContainsKey(id))
            {
                data.enemySavable[id] = instance;
            }
            else
            {
                data.enemySavable.Add(id, instance);
            }
        }
    }
}