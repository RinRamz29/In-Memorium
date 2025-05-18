using System;
using _Memoriam.Script.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Memoriam.Script.Menus
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject firstToSelect;
        [SerializeField] private Button acceptButton;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private AudioMixer masterMixer;

        #region UnityFlow
        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            UnSubscribe();
        }

        private void Initialize()
        {
            EventSystem.current.SetSelectedGameObject(firstToSelect);
            
            acceptButton.onClick.AddListener(ConfirmSettings);
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            languageDropdown.onValueChanged.AddListener(SetLanguage);

            languageDropdown.options.Clear();
            foreach (var lang in LocalizationManager.Instance.languages)
            {
                languageDropdown.options.Add(new TMP_Dropdown.OptionData(lang.ToString()));
            }

            RefreshUI();
        }

        private void UnSubscribe()
        {
            acceptButton.onClick.RemoveListener(ConfirmSettings);
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
            languageDropdown.onValueChanged.RemoveListener(SetLanguage);
        }

        #endregion
        
        private void ConfirmSettings()
        {
            SaveSettings();
        }

        private void SetSfxVolume(float volume)
        {
            masterMixer.SetFloat("SFXVolume", VolumeToDecibels(volume));
            sfxVolumeSlider.value = volume;
        }

        private void SetMusicVolume(float volume)
        {
            masterMixer.SetFloat("MusicVolume", VolumeToDecibels(volume));
            musicVolumeSlider.value = volume;
        }

        private void SetMasterVolume(float volume)
        {
            masterMixer.SetFloat("MasterVolume", VolumeToDecibels(volume));
            masterVolumeSlider.value = volume;
        }

        private void SetResolution(int resolutionIndex)
        {
            resolutionDropdown.value = resolutionIndex;

            var resText = resolutionDropdown.options[resolutionIndex].text;
            var dividedRes = resText.Split('x');

            if (dividedRes.Length != 2)
            {
                Debug.LogWarning($"Formato de resolución inválido: {resText}");
                return;
            }

            if (!int.TryParse(dividedRes[0], out int width) || !int.TryParse(dividedRes[1], out int height))
            {
                Debug.LogWarning($"No se pudo parsear resolución: {resText}");
                return;
            }

            var isSmall = width < 1920 || height < 1080;
            var mode = isSmall ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;

            Screen.SetResolution(width, height, mode);
        }

        private void SetLanguage(int languageIndex)
        {
            languageDropdown.value = languageIndex;
            var selection = LocalizationManager.Instance.languages[languageIndex];

            LocalizationManager.Instance.Translate(selection);
        }

        private void RefreshUI()
        {
            languageDropdown.value = PlayerPrefs.GetInt("LanguageSelection", 0);
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionSelection", 0);
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            PlayerPrefs.SetInt("ResolutionSelection", resolutionDropdown.value);
            PlayerPrefs.SetInt("LanguageSelection", languageDropdown.value);
            PlayerPrefs.Save();
        }

        public static float VolumeToDecibels(float volume)
        {
            if (volume <= 0.0001f)
                return -80f;
            return Mathf.Log10(volume) * 20f;
        }

        public static float DecibelsToVolume(float dB)
        {
            return Mathf.Pow(10f, dB / 20f);
        }
    }
}