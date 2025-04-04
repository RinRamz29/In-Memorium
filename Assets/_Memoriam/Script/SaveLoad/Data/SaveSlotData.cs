using System;
using UnityEngine;

namespace _Memoriam.Script.SaveLoad.Data
{
    [Serializable]
    public class SaveSlotData
    {
        public DateTime SaveDate { get; set; }
        public float PlayerHealth { get; set; }
        public Vector3 PlayerPosition { get; set; }
        public bool HasDoubleJump { get; set; }
        public bool HasDash { get; set; }

        public SaveSlotData(GameData gameData)
        {
            SaveDate = DateTime.Now;
            PlayerHealth = gameData.player.health;
            PlayerPosition = gameData.player.position;
            HasDoubleJump = gameData.player.canDoubleJump;
            HasDash = gameData.player.canDash;
        }
    }
}
