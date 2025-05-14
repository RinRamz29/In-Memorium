using System;
using System.Collections.Generic;
using System.Linq;
using _Memoriam.Script.General;
using _Memoriam.Script.Localization.Helper;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Localization
{
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        private List<ILocalization> _localizations = new List<ILocalization>();
        public List<Languages> languages = new List<Languages>();
        public Languages selectedLanguage;
        
        private void OnEnable()
        {
            _localizations = GetLocalizationInterfaces();
        }

        public void ForceTranslate()
        {
            if (PlayerPrefs.HasKey("LanguageSelection"))
            {
                selectedLanguage = languages[PlayerPrefs.GetInt("LanguageSelection", 0)];
                _localizations = GetLocalizationInterfaces();
                Translate(selectedLanguage);
            }
        }

        public void Translate(Languages language)
        {
            if (_localizations == null || _localizations.Count == 0)
                return;

            foreach (var localized in _localizations)
            {
                localized?.Translate(language);
            }
        }

        private List<ILocalization> GetLocalizationInterfaces()
        {
            var interfaces = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ILocalization>();
            
            return interfaces.ToList();
        }
    }

    public enum Languages
    {
        English,
        Spanish,
        Portuguese,
    }

    [Serializable]
    public class LanguagesClass
    {
        public Languages language;
        public string text;

        public bool TryGetText(Languages languageToPass, out string textOut)
        {
            if (language != languageToPass)
            {
                textOut = null;
                return false;
            }
            
            textOut = text;
            return true;
        }
    }
}