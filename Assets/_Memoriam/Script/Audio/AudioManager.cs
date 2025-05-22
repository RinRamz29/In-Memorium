using System;
using _Memoriam.Script.General;
using _Memoriam.Script.Menus;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Audio
{
    public class AudioManager : Singleton<AudioManager>, IAudioSource
    {
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioDatabase audioDatabase;
        [SerializeField] private AudioMixer sfxMixer;

        [SerializeField] private AudioSource musicAudioSource;

        private void OnEnable()
        {
            if (PlayerPrefs.HasKey("SFXVolume"))
            {
                sfxMixer.SetFloat("SFXVolume", SettingsMenu.VolumeToDecibels(PlayerPrefs.GetFloat("SFXVolume")));
            }

            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                sfxMixer.SetFloat("MusicVolume", SettingsMenu.VolumeToDecibels(PlayerPrefs.GetFloat("MusicVolume")));
            }

            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                sfxMixer.SetFloat("MasterVolume", SettingsMenu.VolumeToDecibels(PlayerPrefs.GetFloat("MasterVolume")));
            }
        }

        public void PlayOneShotSFX(string audioName)
        {
            AudioData audioData = audioDatabase.GetAudio(audioName);
            if (audioData != null && audioData.Clips != null && audioData.Clips.Length > 0)
            {
                sfxAudioSource.PlayOneShot(audioData.Clips[0], audioData.Volume);
            }
            else
            {
                Debug.LogWarning($"Audio '{audioName}' no encontrado o sin clips en AudioDatabase.");
            }
        }

        public void PlayOneShotSFX(AudioClip audioClip)
        {
            sfxAudioSource.PlayOneShot(audioClip);
        }

        public void PlayRandomSFX(string audioName)
        {
            AudioData audioData = audioDatabase.GetAudio(audioName);
            if (audioData != null && audioData.Clips != null && audioData.Clips.Length > 0)
            {
                int index = UnityEngine.Random.Range(0, audioData.Clips.Length);
                AudioClip selectedClip = audioData.Clips[index];
                sfxAudioSource.PlayOneShot(selectedClip, audioData.Volume);
            }
            else
            {
                Debug.LogWarning($"Audio '{audioName}' no encontrado o sin clips en AudioDatabase.");
            }
        }

        public void PlayMusic(string audioName)
        {
            AudioData audioData = audioDatabase.GetAudio(audioName);
            if (audioData == null || audioData.Clips == null || audioData.Clips.Length == 0)
            {
                Debug.LogWarning($"Música '{audioName}' no encontrada en AudioDatabase.");
                return;
            }

            musicAudioSource.Stop();
            musicAudioSource.clip = audioData.Clips[0]; // Para música, se usa el primer clip
            musicAudioSource.volume = audioData.Volume;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }

        public void PlayDoorCloseSFX() => PlayOneShotSFX("DoorCloseSFX");
        public void PlayDoorOpenSFX() => PlayOneShotSFX("DoorOpenSFX");
        public void PlayPauseSFX() => PlayOneShotSFX("PauseSFX");
        public void PlayUIButtonClickSFX() => PlayOneShotSFX("UIButtonClickSFX");
    }
}
