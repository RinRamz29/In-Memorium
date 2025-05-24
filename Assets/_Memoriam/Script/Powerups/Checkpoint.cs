using System;
using System.Collections;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Powerups
{
    public class Checkpoint : MonoBehaviour, ISaveableObject, IPickable
    {
        [field: SerializeField] public TypeOfPickable TypeOfPickable { get; private set; }
        [field: SerializeField] public string ID { get; private set; }
        [SerializeField] private GameObject saveMenu;
        [SerializeField] private ParticleSystem pickupParticlesReversed;

        public static event Action<bool> OnInteractReached;

        private Player.Player _player;
        private bool _playerInRange;
        private bool _isTeleporting;
        
        private void OnEnable()
        {
            InputReader.Instance.PlayerActions.Player.Interact.performed += OnInteractPressed;
        }

        private void OnDisable()
        {
            InputReader.Instance.PlayerActions.Player.Interact.performed -= OnInteractPressed;
        }
        
        private void OnInteractPressed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && _playerInRange)
            {
                _player.LastCheckPoint = transform.position;
                DataPersistentManager.Instance.SaveGame(DataPersistentManager.Instance.SelectedSlot);
                pickupParticlesReversed.Play();
            } 
        } 

        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Player.Player>(out var player))
            {
                _player = player;
                _playerInRange = true;
                OnInteractReached?.Invoke(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<Player.Player>(out var player) && player == _player)
            {
                _playerInRange = false;
                _player = null;
                OnInteractReached?.Invoke(false);
            }
        }

        public void Pick(GameObject player)
        {
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