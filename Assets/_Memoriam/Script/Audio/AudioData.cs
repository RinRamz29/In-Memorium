using UnityEngine;

namespace TerrorConsole
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData")]

    public class AudioData : ScriptableObject
    {
        public string AudioName;
        public AudioClip AudioClip;
        public float Volume = 1;
    }
}
