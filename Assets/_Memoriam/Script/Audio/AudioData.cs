using UnityEngine;

namespace _Memoriam.Script.Audio
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData")]

    public class AudioData : ScriptableObject
    {
        public string AudioName;
        public AudioClip AudioClip;
        public float Volume = 1;
    }
}
