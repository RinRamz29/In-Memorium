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
                SFXVolume = 1f;
            }

            float volumeDB = Mathf.Lerp(-80f, 0f, SFXVolume);
            _sfxMixer.SetFloat("SFXVolume", volumeDB);
        }

        public void PlayOneShotSFX(string audioName)
        {
            AudioData audioData = _audioDatabase.GetAudio(audioName);
            if (audioData != null && audioData.Clips != null && audioData.Clips.Length > 0)
            {
                _sfxAudioSource.PlayOneShot(audioData.Clips[0], audioData.Volume);
            }
            else
            {
                Debug.LogWarning($"Audio '{audioName}' no encontrado o sin clips en AudioDatabase.");
            }
        }

        public void PlayRandomSFX(string audioName)
        {
            AudioData audioData = _audioDatabase.GetAudio(audioName);
            if (audioData != null && audioData.Clips != null && audioData.Clips.Length > 0)
            {
                int index = UnityEngine.Random.Range(0, audioData.Clips.Length);
                AudioClip selectedClip = audioData.Clips[index];
                _sfxAudioSource.PlayOneShot(selectedClip, audioData.Volume);
            }
            else
            {
                Debug.LogWarning($"Audio '{audioName}' no encontrado o sin clips en AudioDatabase.");
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                sfxMuted = !sfxMuted;
                _sfxMixer.SetFloat("SFXVolume", sfxMuted ? -80f : Mathf.Lerp(-80f, 0f, SFXVolume));
            }
        }

        public void SetSFXVolume(float newVolume)
        {
            newVolume = Mathf.Clamp(newVolume, 0f, 1f);
            float volumeDB = Mathf.Lerp(-80f, 0f, newVolume);
            _sfxMixer.SetFloat("SFXVolume", volumeDB);
            PlayerPrefs.SetFloat("sfxVol", newVolume);
        }

        public void PlayMusic(string audioName)
        {
            AudioData audioData = _audioDatabase.GetAudio(audioName);
            if (audioData == null || audioData.Clips == null || audioData.Clips.Length == 0)
            {
                Debug.LogWarning($"Música '{audioName}' no encontrada en AudioDatabase.");
                return;
            }

            _musicAudioSource.Stop();
            _musicAudioSource.clip = audioData.Clips[0]; // Para música, se usa el primer clip
            _musicAudioSource.volume = audioData.Volume;
            _musicAudioSource.loop = true;
            _musicAudioSource.Play();
        }

        public void PlayDoorCloseSFX() => PlayOneShotSFX("DoorCloseSFX");
        public void PlayDoorOpenSFX() => PlayOneShotSFX("DoorOpenSFX");
        public void PlayPauseSFX() => PlayOneShotSFX("PauseSFX");
        public void PlayUIButtonClickSFX() => PlayOneShotSFX("UIButtonClickSFX");
    }
}
