using System;
using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public interface IPickable
    {
        public TypeOfPowerUp TypeOfPowerUp { get; }
        
        public void Pick();
    }
    
    [Serializable]    
    public enum TypeOfPowerUp
    {
        DoubleJump,
        Dash,
    }
}