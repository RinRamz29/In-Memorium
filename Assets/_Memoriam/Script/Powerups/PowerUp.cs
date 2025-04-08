using System;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public class PowerUp : MonoBehaviour, IPickable, ISaveableObject
    {
        [field: SerializeField] public TypeOfPickable TypeOfPickable { get; private set; }
        [field: SerializeField] public string ID { get; private set; }

        public void Pick(GameObject player)
        {
            if (player == null)
                return;

            if (player.TryGetComponent<Player.Player>(out var playerPlayer))
            {
                switch (TypeOfPickable)
                {
                    case TypeOfPickable.Dash:
                        playerPlayer.CanDash = true;
                        break;
                    case TypeOfPickable.DoubleJump:
                        playerPlayer.CanDoubleJump = true;
                        break;
                }
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
}