using UnityEngine;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;

namespace _Memoriam.Script.Enemies.Components
{
    public class EnemySaveLoad : MonoBehaviour, ISaveableObject
    {
        [field: SerializeField] public string id { get; set; } // El ID único para guardado
        private BaseEnemy _baseEnemy; // Referencia si necesitas acceder a otros datos para guardar/cargar

        public void Initialize(BaseEnemy baseEnemy)
        {
            _baseEnemy = baseEnemy;
            // Generar un ID único si no está asignado (esto es solo un ejemplo, necesitas una estrategia robusta para IDs únicos)
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
                #if UNITY_EDITOR
                // Si estás en el editor, es bueno marcar el objeto como 'dirty' para que el ID se guarde en la escena/prefab.
                if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
                #endif
                Debug.LogWarning($"Enemy {gameObject.name} had no ID, generated a new one: {id}. Ensure IDs are unique and persistent.", this);
            }
        }
        
        public void LoadData(GameData data)
        {
            if (data.enemySavable.TryGetValue(id, out SavableEnemy enemyData))
            {
                gameObject.SetActive(enemyData.isAlive);
                transform.position = enemyData.position;
                // Aquí también podrías cargar la salud actual del enemigo si la guardas
                if (_baseEnemy.Stats != null)
                {
                    // Suponiendo que SavableEnemy tiene un campo CurrentHealth
                    // _baseEnemy.Stats.CurrentHealth = enemyData.CurrentHealth; 
                }
            }
            else
            {
                // Opcional: ¿Qué hacer si no hay datos guardados para este ID?
                // Podría ser un nuevo enemigo o uno que no se guardó previamente.
                // gameObject.SetActive(true); // Asegurar que esté activo si es su estado por defecto
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
                isAlive = gameObject.activeInHierarchy, // O podrías tener una propiedad IsAlive más explícita
                position = transform.position,
                // Aquí también podrías guardar la salud actual
                // CurrentHealth = _baseEnemy.Stats.CurrentHealth 
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