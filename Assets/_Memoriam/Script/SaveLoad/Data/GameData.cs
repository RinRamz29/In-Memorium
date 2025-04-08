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
        public Vector3 lastCheckpoint;
        public bool canDoubleJump;
        public bool canDash;
        public float health;
    }

    [Serializable]
    public class GamePlayMenuSaveData
    {
        public bool canDoubleJump;
        public bool canDash;
        public float health;
    }
    
    [Serializable]
    public class GameData 
    {
        public SavablePlayer player;
        public SerializableDictionary<string, SavableEnemy> EnemySavable;
        public SerializableDictionary<string, bool> pickableSavable;
        public GamePlayMenuSaveData gamePlayMenu;        

        public GameData()
        {
            pickableSavable = new SerializableDictionary<string, bool>();
            player = new SavablePlayer();
            EnemySavable = new SerializableDictionary<string, SavableEnemy>();
            gamePlayMenu = new GamePlayMenuSaveData();
        }
    }
}
