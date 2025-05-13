using System;
using System.Collections.Generic;
using _Memoriam.Script.Player;
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
        public Vector3 lastCheckpoint;
        public int level;
        public float xp;
        public float health;
        public PlayerAbilities abilities;
    }

    [Serializable]
    public class TutorialData
    {
        public int currentTutoIndex;
        public bool isTutoFinished;
        public bool isOn;
    }
    
    [Serializable]
    public class GameData 
    {
        public SavablePlayer player = new();
        public TutorialData tutoData = new();
        public SerializableDictionary<string, SavableEnemy> enemySavable = new();
        public SerializableDictionary<string, bool> pickableSavable = new();
    }
}
