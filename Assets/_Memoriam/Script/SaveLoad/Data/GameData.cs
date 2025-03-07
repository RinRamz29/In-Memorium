using System;
using System.Collections.Generic;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.Serlalization.Serialization_Types;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.SaveLoad.Data
{
    [Serializable]
    public class SavableEnemy
    {
        public Vector3 position;
        public bool isAlive;
    }
    
    [Serializable]
    public class SavablePlayer
    {
        public Vector3 position;
        public bool canDoubleJump;
        public bool canDash;
        public float health;
    }
    
    [Serializable]
    public class GameData 
    {
        public SavablePlayer player;
        public SerializableDictionary<string, SavableEnemy> EnemySavable;
        public SerializableDictionary<TypeOfPowerUp, bool> powerUpSavable;

        public GameData()
        {
            powerUpSavable = new SerializableDictionary<TypeOfPowerUp, bool>();
            player = new SavablePlayer();
            EnemySavable = new SerializableDictionary<string, SavableEnemy>();
        }
    }
}
