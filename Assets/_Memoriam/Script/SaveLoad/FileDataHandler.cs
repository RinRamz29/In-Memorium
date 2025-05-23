using System;
using System.IO;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

namespace _Memoriam.Script.SaveLoad
{
    public class FileDataHandler
    {
        private string _dataDirthPath = Application.persistentDataPath;
        private string _baseFileName = "saveData_{0}";
        private readonly string _encryptionCodeWord = "rubenEsSexy";
        private const int MaxSlots = 3;

        public GameData LoadData(int slot)
        {
            var fullPath = Path.Combine(_dataDirthPath, string.Format(_baseFileName, slot));
            GameData loadedData = null;

            if (File.Exists(fullPath))
            {
                try
                {
                    var dataToLoad = "";
                    
                    using (var stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }
                    
                    dataToLoad = Encrypt(dataToLoad);
                    
                    loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            return loadedData;
        }

        public void EraseSaveData(int slot)
        {
            var fullPath = Path.Combine(_dataDirthPath, string.Format(_baseFileName, slot));

            try
            {
                File.Delete(fullPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void SaveData(GameData data, int slot)
        {
            var fullPath = Path.Combine(_dataDirthPath, string.Format(_baseFileName, slot));

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                var dataToStore = JsonUtility.ToJson(data, true);
                dataToStore = Encrypt(dataToStore);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool DoesSaveExist(int slot)
        {
            var fullPath = Path.Combine(_dataDirthPath, string.Format(_baseFileName, slot));
            return File.Exists(fullPath);
        }

        private string Encrypt(string data)
        {
            var modifiedData = "";

            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)(data[i] ^ _encryptionCodeWord[i % _encryptionCodeWord.Length]);
            }
            return modifiedData;
        }
    }
}
