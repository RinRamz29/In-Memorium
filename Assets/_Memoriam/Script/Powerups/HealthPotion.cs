using _Memoriam.Script.Player;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

public class HealthPotion : MonoBehaviour, IPickable, ISaveableObject
{
    [field: SerializeField] public TypeOfPickable TypeOfPickable { get; private set; }
    [SerializeField] private float healAmount;
    
    public void Pick(GameObject player)
    {
        if (player.TryGetComponent(out IPlayer playerController))
        {
            playerController.ReceiveHeal(healAmount);
        }
        
        gameObject.SetActive(false);
    }

    public void LoadData(GameData data)
    {
        if (data.healthPotionSavable.TryGetValue(TypeOfPickable, out var isActive))
        {
            gameObject.SetActive(isActive);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.healthPotionSavable.ContainsKey(TypeOfPickable))
        {
            data.healthPotionSavable.Remove(TypeOfPickable);
        }
            
        data.healthPotionSavable.Add(TypeOfPickable, gameObject.activeInHierarchy);
    }
}
