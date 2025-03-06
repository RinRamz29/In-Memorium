using System;
using System.Collections.Generic;
using _Memoriam.Script.Serlalization.Serialization_Types;
using NUnit.Framework;
using UnityEngine;

namespace _Memoriam.Script.SaveLoad.Data
{
    [Serializable]
    public class SavableEnemy
    {
        public Vector3 position;
        public bool isAlive;
    }
    
    
    [Serializable]
    public class GameData 
    {
        public Vector3 playerPosition;
        public SerializableDictionary<string, SavableEnemy> EnemySavable;

        public GameData()
        {
            playerPosition = Vector3.zero;
            EnemySavable = new SerializableDictionary<string, SavableEnemy>();
        }
    }
}
