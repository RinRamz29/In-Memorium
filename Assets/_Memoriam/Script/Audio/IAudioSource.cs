using UnityEngine;

namespace TerrorConsole
{
    public interface IAudioSource
    {
        float SFXVolume { get; }
        void SetSFXVolume(float newVolume);
        void PlayDoorCloseSFX();
        void PlayDoorOpenSFX();
        void PlayPauseSFX();
        void PlayUIButtonClickSFX();
    }
}
