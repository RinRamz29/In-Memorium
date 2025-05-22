using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Localization;
using _Memoriam.Script.Managers;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Memoriam.Script.Tutorial
{
    public class TutorialManager : Singleton<TutorialManager>, ISaveableObject, ILocalization
    {
        public List<TutorialStep> steps;
        [field: SerializeField] public TMP_Text TextToTranslateTMP { get; set; }
        [SerializeField] public Image icon;
        [SerializeField] public GameObject canvas;

        public Languages currentLanguage = Languages.English;
        public int CurrentStepIndex { get; private set; }
        public bool TutoActive { get; set; }
        public static event Action<int> OnTutorialLoaded;
        public static event Action OnTutorialEnded;

        protected override void Awake()
        {
            base.Awake();

            CheckForInput();
            SetCanvas(true);
            RefreshUI();
        }

        private void Update()
        {
            if (GameStateManager.Instance.GameCurrentState == GameStateManager.GameState.OnGameplay)
                CheckForInput();
        }

        public void NextStep()
        {
            CurrentStepIndex++;

            if (CheckIfCompleted())
            {
                OnTutorialEnded?.Invoke();
                return;
            }

            RefreshUI();
        }

        public bool CheckIfCompleted()
        {
            return CurrentStepIndex >= steps.Count;
        }

        private void RefreshUI()
        {
            if (CheckIfCompleted())
                return;
            
            steps[CurrentStepIndex].TryGetInt(currentLanguage, out var idx);
            TextToTranslateTMP.text = steps[CurrentStepIndex].languages[idx].text;
            CheckForInput();
        }

        private void CheckForInput()
        {
            if (CheckIfCompleted())
                return;
            
            if (InputReader.Instance.ControlTypes == InputReader.ControlType.Control)
            {
                if (steps[CurrentStepIndex].icon.Count > 0)
                {
                    var sprite = steps[CurrentStepIndex].icon[0];
                    icon.sprite = sprite;
                    icon.enabled = true;
                    icon.rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
                    icon.rectTransform.sizeDelta *= 5;
                }
                else
                    icon.enabled = false;
            }
            else if (InputReader.Instance.ControlTypes == InputReader.ControlType.KeyboardMouse)
            {
                if (steps[CurrentStepIndex].icon.Count > 0)
                {
                    var sprite = steps[CurrentStepIndex].icon[1];
                    icon.sprite = sprite;
                    icon.enabled = true;
                    icon.rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
                    icon.rectTransform.sizeDelta *= 5;
                }
                else
                    icon.enabled = false;
            }
        }

        public void Translate(Languages language)
        {
            if (CheckIfCompleted())
                return;
            
            if (steps[CurrentStepIndex].TryGetText(language, out var translatedText))
            {
                currentLanguage = language;
                TextToTranslateTMP.text = translatedText;
            }
        }

        public void ResetTutorial(bool isOn)
        {
            CurrentStepIndex = 0;
            SetCanvas(isOn);
            RefreshUI();
            OnTutorialLoaded?.Invoke(CurrentStepIndex);
        }

        public void SetCanvas(bool isOn)
        {
            canvas.SetActive(isOn);
        }

        public void LoadData(GameData data)
        {
            CurrentStepIndex = data.tutoData.currentTutoIndex;
            SetCanvas(data.tutoData.isOn);
            RefreshUI();

            OnTutorialLoaded?.Invoke(CurrentStepIndex);
        }

        public void SaveData(ref GameData data)
        {
            var tuto = new TutorialData()
            {
                currentTutoIndex = CurrentStepIndex == 0 ? steps.Count : CurrentStepIndex,
                isOn = canvas.activeInHierarchy,
            };
            data.tutoData = tuto;
        }
    }

    [Serializable]
    public class TutorialStep
    {
        public List<Sprite> icon;
        public List<LanguagesClass> languages;

        public enum ActionType
        {
            Jump,
            Dash,
            DoubleJump,
            LightAttack,
            HeavyAttack,
            Combo,
            Interact
        }

        public ActionType action;

        public bool TryGetText(Languages languageToPass, out string textOut)
        {
            foreach (var lang in languages)
            {
                if (lang.language == languageToPass)
                {
                    textOut = lang.text;
                    return true;
                }
            }

            textOut = null;
            return false;
        }

        public int TryGetInt(Languages languageToPass, out int index)
        {
            var idx = 0;
            for (int i = 0; i < languages.Count; i++)
            {
                index = i;
                if (languages[i].language == languageToPass)
                {
                    index = i;
                    return idx;
                }
            }

            index = 0;
            return idx;
        }
    }
}