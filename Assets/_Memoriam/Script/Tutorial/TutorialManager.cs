using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Memoriam.Script.Tutorial
{
    public class TutorialManager : Singleton<TutorialManager>, ISaveableObject
    {
        public List<TutorialStep> steps;
        [field: SerializeField] public int CurrentStepIndex { get; private set; }

        [SerializeField] public TextMeshProUGUI instructionText;
        [SerializeField] public Image icon;
        [SerializeField] public GameObject canvas;

        public static event Action<TutorialStep> OnTutorialLoaded;

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
            if (CurrentStepIndex + 1 < steps.Count)
            {
                CurrentStepIndex++;
                instructionText.text = steps[CurrentStepIndex].instruction;
                CheckForInput();
            }
            else
            {
                EndTutorial();
            }
        }

        private void RefreshUI()
        {
            if (CurrentStepIndex >= 0 && CurrentStepIndex < steps.Count)
            {
                instructionText.text = steps[CurrentStepIndex].instruction;
                CheckForInput();
            }
        }

        private void CheckForInput()
        {
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

        public void ResetTutorial()
        {
            CurrentStepIndex = 0;
            SetCanvas(true);
            RefreshUI();
            OnTutorialLoaded?.Invoke(steps[CurrentStepIndex]);
        }

        public void SetCanvas(bool isOn)
        {
            canvas.SetActive(isOn);
        }

        public void EndTutorial()
        {
            SetCanvas(false);
        }

        public void LoadData(GameData data)
        {
            CurrentStepIndex = data.tutoData.currentTutoIndex;

            SetCanvas(data.tutoData.isOn);
            RefreshUI();
            OnTutorialLoaded?.Invoke(steps[CurrentStepIndex]);
        }

        public void SaveData(ref GameData data)
        {
            var tuto = new TutorialData()
            {
                currentTutoIndex = CurrentStepIndex,
                isOn = canvas.activeInHierarchy,
            };
            data.tutoData = tuto;
        }
    }

    [Serializable]
    public class TutorialStep
    {
        public string instruction;
        public List<Sprite> icon;

        public enum ActionType
        {
            Move,
            Jump,
            Dash,
            DoubleJump,
            LightAttack,
            HeavyAttack,
            Combo,
            Interact
        }

        public ActionType action;
    }
}