using System;
using System.Collections;
using System.Collections.Generic;
using _Memoriam.Script.Audio;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public class PowerUp : MonoBehaviour, IPickable, ISaveableObject
    {
        [field: SerializeField] public TypeOfPickable TypeOfPickable { get; private set; }
        [field: SerializeField] public string ID { get; private set; }
        [SerializeField] private ParticleSystem pickupParticles;

        public void Pick(GameObject player)
        {
            if (player == null)
                return;
            
            AudioManager.Instance.PlayOneShotSFX("Pickup");
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