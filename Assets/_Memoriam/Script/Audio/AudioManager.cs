using _Memoriam.Script.General;
using UnityEngine;
using UnityEngine.Audio;

namespace _Memoriam.Script.Audio
{
    public class AudioManager : Singleton<AudioManager>, IAudioSource
    {
        [SerializeField] private AudioSource _sfxAudioSource;
        [SerializeField] private AudioDatabase _audioDatabase;
        [SerializeField] private AudioMixer _sfxMixer;

        [SerializeField] private AudioSource _musicAudioSource;
        [SerializeField] private AudioMixer _musicMixer;

        public float SFXVolume { get; private set; }

        private bool sfxMuted = false;


        protected override void Awake()
        {
            base.Awake();

            SFXVolume = PlayerPrefs.HasKey("sfxVol") ? PlayerPrefs.GetFloat("sfxVol") : 1f;
        }

        private void Start()
        {
            InitializeSFX();
        }

        private void InitializeSFX()
        {
            if (PlayerPrefs.HasKey("sfxVol"))
            {
                SFXVolume = PlayerPrefs.GetFloat("sfxVol");
            }
            else
            {
                SFXVolume = 1f; // Volumen por defecto si nunca se ha guardado
            }

            float volumeDB = Mathf.Lerp(-80f, 0f, SFXVolume);
            _sfxMixer.SetFloat("SFXVolume", volumeDB);
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

        //MODIFICACI�N: Ahora este m�todo es P�BLICO para que pueda ser accedido desde otros scripts
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

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                sfxMuted = !sfxMuted;

                if (sfxMuted)
                {
                    _sfxMixer.SetFloat("SFXVolume", -80f);
                    Debug.Log("SFX OFF");
                }
                else
                {
                    _sfxMixer.SetFloat("SFXVolume", Mathf.Lerp(-80f, 0f, SFXVolume));
                    Debug.Log("SFX ON");
                }
            }
        }

        /// <summary>
        /// New volume represented in a range from 0 to 1
        /// </summary>
        /// <param name="newVolume"></param>
        public void SetSFXVolume(float newVolume)
        {
            newVolume = Mathf.Clamp(newVolume, 0f, 1f); // Asegura que el valor est� entre 0 y 1
            float volumeDB = Mathf.Lerp(-80f, 0f, newVolume); // Convierte de un rango de 0-1 a -80dB (silencio) a 0dB (volumen m�ximo)

            _sfxMixer.SetFloat("SFXVolume", volumeDB); // Asigna el volumen en el Audio Mixer
            PlayerPrefs.SetFloat("sfxVol", newVolume); // Guarda el volumen para la pr�xima vez que se inicie el juego
        }

        public void PlayMusic(string audioName)
        {
            AudioData audioData = _audioDatabase.GetAudio(audioName);

            if (audioData == null)
            {
                Debug.LogWarning($"M�sica '{audioName}' no encontrada en el AudioDatabase.");
                return;
            }

            Debug.Log($"[AUDIO] Cambiando m�sica a: {audioName}");

            _musicAudioSource.Stop();
            _musicAudioSource.clip = audioData.AudioClip;
            _musicAudioSource.volume = audioData.Volume;
            _musicAudioSource.loop = true;
            _musicAudioSource.Play();
        }

        public void SetMusicVolume(float newVolume)
        {
            newVolume = Mathf.Clamp(newVolume, 0f, 1f);
            float volumeDB = Mathf.Lerp(-80f, 0f, newVolume);
            _musicMixer.SetFloat("MusicVolume", volumeDB);
            PlayerPrefs.SetFloat("musicVol", newVolume);
            PlayerPrefs.Save();
        }
    }
}
