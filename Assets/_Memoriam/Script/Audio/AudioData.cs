using System;
using UnityEngine;

namespace _Memoriam.Script.Audio
{
    [Serializable]
    public class AudioData
    {
        public string AudioName;
        public AudioClip[] Clips;
        public float Volume = 1;
    }
}