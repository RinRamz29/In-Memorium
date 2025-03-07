using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public class Checkpoint : MonoBehaviour, ISaveableObject, IPickable
    {
        [field: SerializeField] public TypeOfPickable TypeOfPickable { get; private set; }
        
        public void Pick(GameObject player)
        {
            if (player.TryGetComponent(out Player.Player playerController))
            {
                playerController.LastCheckPoint = transform.position;
            }
            
            DataPersistentManager.Instance.SaveGame();
            gameObject.SetActive(false);
        }

        public void LoadData(GameData data)
        {
            if (data.checkPointSavable.TryGetValue(TypeOfPickable, out var isActive))
            {
                gameObject.SetActive(isActive);
            }
        }

        public void SaveData(ref GameData data)
        {
            if (data.checkPointSavable.ContainsKey(TypeOfPickable))
            {
                data.checkPointSavable.Remove(TypeOfPickable);
            }
            
            data.checkPointSavable.Add(TypeOfPickable, gameObject.activeInHierarchy);
        }
    }
}