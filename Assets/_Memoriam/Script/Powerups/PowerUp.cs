using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public class PowerUp : MonoBehaviour, IPickable, ISaveableObject
    {
        [field: SerializeField] public TypeOfPowerUp TypeOfPowerUp { get; private set; }
        
        public void Pick()
        {
            gameObject.SetActive(false);
        }

        public void LoadData(GameData data)
        {
            if (data.powerUpSavable.TryGetValue(TypeOfPowerUp, out var isActive))
            {
                gameObject.SetActive(isActive);
            }
        }

        public void SaveData(ref GameData data)
        {
            if (data.powerUpSavable.ContainsKey(TypeOfPowerUp))
            {
                data.powerUpSavable.Remove(TypeOfPowerUp);
            }
            
            data.powerUpSavable.Add(TypeOfPowerUp, gameObject.activeInHierarchy);
        }
    }
}