using System;
using _Memoriam.Script.Player;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

public class HealthPotion : MonoBehaviour, IPickable, ISaveableObject
{
    [field: SerializeField] public TypeOfPickable TypeOfPickable { get; private set; }
    [SerializeField] private float healAmount;
    [field: SerializeField] public string ID { get; private set; }

    public void Pick(GameObject player)
    {
        if (player.TryGetComponent(out IPlayer playerController))
        {
            playerController.ReceiveHeal(healAmount);
        }
        
        gameObject.SetActive(false);
    }

    [ContextMenu("Generate ID")]
    public void GenerateID()
    {
        ID = Guid.NewGuid().ToString();
    }

    public void LoadData(GameData data)
    {
        if (data.pickableSavable.TryGetValue(ID, out var isActive))
        {
            gameObject.SetActive(isActive);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.pickableSavable.ContainsKey(ID))
        {
            data.pickableSavable.Remove(ID);
        }

        data.pickableSavable.Add(ID, gameObject.activeInHierarchy);
    }
}
