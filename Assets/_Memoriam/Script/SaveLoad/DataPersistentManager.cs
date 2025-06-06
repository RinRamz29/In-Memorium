using System.Collections.Generic;
using System.Linq;
using _Memoriam.Script.General;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;
using UnityEngine.Scripting;

namespace _Memoriam.Script.SaveLoad
{
    [Preserve]    
    public class DataPersistentManager : Singleton<DataPersistentManager>
    {
        private GameData _gameData;
        private int _currentSlot = 1;
        private List<ISaveableObject> _saveableObjects;
        public FileDataHandler FileDataHandler { get; private set; }
        public int SelectedSlot { get; set; }
        
        protected override void Awake()
        {
            base.Awake();
            _saveableObjects = FindAllSaveableObjects();
            FileDataHandler = new FileDataHandler();
        }

        public void NewGame()
        {
            _gameData = new GameData();
        }
        
        public void DeleteSave(int slot)
        {
            FileDataHandler.EraseSaveData(slot);
        }

        public void LoadGame(int slot)
        {
            _currentSlot = slot;

            if (FileDataHandler.DoesSaveExist(slot) == false)
            {
                return;
            }
            
            _gameData = FileDataHandler.LoadData(slot);
            
            var savesObjects = FindAllSaveableObjects();
            
            foreach (var savedObj in savesObjects)
            {
                savedObj.LoadData(_gameData);
            }
        }

        public GameData GetSlotData(int slot)
        {
            if (FileDataHandler.DoesSaveExist(slot) == false)
                return null;
            
            var data = FileDataHandler.LoadData(slot);
            
            return data;
        }
        
        public void SaveGame(int slot)
        {
            _currentSlot = slot;
            _saveableObjects = FindAllSaveableObjects();
            
            foreach (var savedObj in _saveableObjects)
            {
                savedObj.SaveData(ref _gameData);
            }
            
            
            FileDataHandler.SaveData(_gameData, slot);
        }

        private List<ISaveableObject> FindAllSaveableObjects()
        {
            var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)   
                .OfType<ISaveableObject>();

            
            return saveables.ToList();
        }

        public bool DoesSaveExist(int slot)
        {
            return FileDataHandler.DoesSaveExist(slot);
        }
    }
}
