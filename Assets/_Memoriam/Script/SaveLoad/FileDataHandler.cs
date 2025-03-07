using System;
using System.IO;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

namespace _Memoriam.Script.SaveLoad
{
    public class FileDataHandler
    {
        private string _dataDirthPath = Application.persistentDataPath;
        private string _fileName = "saveData";
        private readonly string _encryptionCodeWord = "rubenEsGay";

        public GameData LoadData()
        {
            var fullPath = Path.Combine(_dataDirthPath, _fileName);
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
                    Debug.Log(dataToLoad);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            return loadedData;
        }

        public void SaveData(GameData data)
        {
            var fullPath = Path.Combine(_dataDirthPath, _fileName);

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