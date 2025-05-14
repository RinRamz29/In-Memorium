using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Localization.Helper
{
    public class LocalizationHelper : MonoBehaviour, ILocalization
    {
        [field: SerializeField] public List<LanguagesClass> languages = new List<LanguagesClass>();
        [field: SerializeField] public TMP_Text TextToTranslateTMP { get; set; }

        private void OnEnable()
        {
            if (TextToTranslateTMP == null)
                TextToTranslateTMP = GetComponent<TMP_Text>();
        }

        public void Translate(Languages language)
        {
            foreach (var lang in languages)
            {
                if (lang.TryGetText(language, out var txt))
                {
                    TextToTranslateTMP.text = txt;
                    break;
                }
            }
        }
    }
}