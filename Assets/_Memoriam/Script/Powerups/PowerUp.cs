using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public class PowerUp : MonoBehaviour, IPickable, ISaveableObject
    {
        [field: SerializeField] public TypeOfPickable TypeOfPickable { get; private set; }
        
        public void Pick(GameObject player)
        {
            gameObject.SetActive(false);
        }

        public void LoadData(GameData data)
        {
            if (data.powerUpSavable.TryGetValue(TypeOfPickable, out var isActive))
            {
                gameObject.SetActive(isActive);
            }
        }

        public void SaveData(ref GameData data)
        {
            if (data.powerUpSavable.ContainsKey(TypeOfPickable))
            {
                data.powerUpSavable.Remove(TypeOfPickable);
            }
            
            data.powerUpSavable.Add(TypeOfPickable, gameObject.activeInHierarchy);
        }
    }
}