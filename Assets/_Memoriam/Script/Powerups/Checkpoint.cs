using System;
using System.Collections;
using _Memoriam.Script.Managers;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Powerups
{
    public class Checkpoint : MonoBehaviour, ISaveableObject, IPickable
    {
        [field: SerializeField] public TypeOfPickable TypeOfPickable { get; private set; }
        [field: SerializeField] public string ID { get; private set; }

        [SerializeField] private GameObject saveMenu;
        [SerializeField] private ParticleSystem pickupParticles;

        public void Pick(GameObject player)
        {
            if (player.TryGetComponent(out Player.Player playerController))
            {
                playerController.LastCheckPoint = transform.position;
            }
            
            DataPersistentManager.Instance.SaveGame(DataPersistentManager.Instance.SelectedSlot);
            StartCoroutine(WaitForParticles());
        }

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            ID = Guid.NewGuid().ToString();
        }

        private IEnumerator WaitForParticles()
        {
            pickupParticles?.Play();
            GetComponent<Collider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(1f);
            gameObject.SetActive(false);
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