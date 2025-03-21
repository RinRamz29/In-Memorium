using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Windows;

namespace TerrorConsole
{
    public class AudioManager : MonoBehaviour, IAudioSource
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource _sfxAudioSource;
        [SerializeField] private AudioDatabase _audioDatabase;
        [SerializeField] private AudioMixer _sfxMixer;

        public float SFXVolume { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeSFX();
        }

        private void InitializeSFX()
        {
            SFXVolume = PlayerPrefs.GetFloat("sfxVol", SFXVolume);
            _sfxMixer.SetFloat("SFXVolume", Mathf.Lerp(-80f, 0f, SFXVolume));
        }

        public void PlayDoorCloseSFX()
        {
            PlayOneShotSFX("DoorCloseSFX");
        }

        public void PlayDoorOpenSFX()
        {
            PlayOneShotSFX("DoorOpenSFX");
        }

        public void PlayPauseSFX()
        {
            PlayOneShotSFX("PauseSFX");
        }

        public void PlayUIButtonClickSFX()
        {
            PlayOneShotSFX("UIButtonClickSFX");
        }

        //MODIFICACIÓN: Ahora este método es PÚBLICO para que pueda ser accedido desde otros scripts
        public void PlayOneShotSFX(string audioName)
        {
            AudioData audioData = _audioDatabase.GetAudio(audioName);
            if (audioData != null)
            {
                _sfxAudioSource.PlayOneShot(audioData.AudioClip, audioData.Volume);
            }
            else
            {
                Debug.LogWarning($"Audio '{audioName}' no encontrado en AudioDatabase.");
            }
        }

        /// <summary>
        /// New volume represented in a range from 0 to 1
        /// </summary>
        /// <param name="newVolume"></param>
        public void SetSFXVolume(float newVolume)
        {
            newVolume = Mathf.Clamp(newVolume, 0f, 1f); // Asegura que el valor esté entre 0 y 1
            float volumeDB = Mathf.Lerp(-80f, 0f, newVolume); // Convierte de un rango de 0-1 a -80dB (silencio) a 0dB (volumen máximo)

            _sfxMixer.SetFloat("SFXVolume", volumeDB); // Asigna el volumen en el Audio Mixer
            PlayerPrefs.SetFloat("sfxVol", newVolume); // Guarda el volumen para la próxima vez que se inicie el juego
        }
    }
}
