using System;
using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public interface IPickable
    {
        public TypeOfPickable TypeOfPickable { get; }
        public string ID { get; }

        public void Pick(GameObject player);

        public void GenerateID();
    }

    [Serializable]
    public enum TypeOfPickable
    {
        DoubleJump,
        Dash,
        ShadowForm, 
        Grapple,
        CheckPoint,
        HealthPotion,
    }
}