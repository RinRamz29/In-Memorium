using System;
using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public interface IPickable
    {
        public TypeOfPickable TypeOfPickable { get; }

        public void Pick(GameObject player);
    }

    [Serializable]
    public enum TypeOfPickable
    {
        DoubleJump,
        Dash,
        CheckPoint,
    }
}