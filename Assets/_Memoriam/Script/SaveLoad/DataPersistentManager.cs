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
        private List<ISaveableObject> _saveableObjects; 
        private FileDataHandler _fileDataHandler;
        public bool IsNewGame { get; set; }
        
        public GameData GameData => _gameData;

        protected override void Awake()
        {
            base.Awake();
            _saveableObjects = FindAllSaveableObjects();
            _fileDataHandler = new FileDataHandler();
        }

        public void NewGame()
        {
            _gameData = new GameData();
        }

        public void LoadGame()
        {
            _gameData = _fileDataHandler.LoadData();
            
            if (_gameData == null)
            {
                Debug.LogError("No save data found");
                return;
            }

            var savesObjects = FindAllSaveableObjects();
            
            foreach (var savedObj in savesObjects)
            {
                savedObj.LoadData(_gameData);
            }
        }
        
        public void SaveGame()
        {
            _saveableObjects = FindAllSaveableObjects();
            
            foreach (var savedObj in _saveableObjects)
            {
                savedObj.SaveData(ref _gameData);
            }
            
            _fileDataHandler.SaveData(_gameData);
        }

        private List<ISaveableObject> FindAllSaveableObjects()
        {
            var saveableObjects = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveableObject>();
            
            return saveableObjects.ToList();
        }
    }
}