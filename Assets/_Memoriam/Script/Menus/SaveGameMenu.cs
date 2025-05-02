using System;
using _Memoriam.Script.Managers;
using _Memoriam.Script.SaveLoad;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Memoriam.Script.Menus
{
    public class SaveGameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject firstToSelect;

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(firstToSelect);
        }   

        public void Save(int slot)
        {
            DataPersistentManager.Instance.SaveGame(slot);
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnGameplay);
        }
    }
}