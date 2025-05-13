using System;
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
        public bool isFinished;
        public int CurrentStepIndex { get; set; } 
        public TutorialStep CurrentStep { get; private set; }
        [SerializeField] public TextMeshProUGUI instructionText;
        [SerializeField] public Image icon;
        [SerializeField] public GameObject canvas;

        protected override void Awake()
        {
            base.Awake();
            if (isFinished)
                return;
            
            CurrentStep = steps[CurrentStepIndex];
            SetTutorialUI(true);
            ShowStep();
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
                CurrentStep = steps[CurrentStepIndex];
                ShowStep();
            }
            else
            {
                EndTutorial();
            }
        }
    
        public void ShowStep() 
        {
            instructionText.text = CurrentStep.instruction;
        }

        public void CheckForInput()
        {
            if (InputReader.Instance.ControlTypes == InputReader.ControlType.Control)
            {
                if (CurrentStep.icon.Count > 0)
                {
                    icon.rectTransform.rect.Set(CurrentStep.icon[0].rect.x, CurrentStep.icon[0].rect.y, CurrentStep.icon[0].rect.width, CurrentStep.icon[0].rect.height);
                    icon.enabled = true;
                    icon.sprite = CurrentStep.icon[0];
                }
                else
                    icon.enabled = false;
            }
            else if (InputReader.Instance.ControlTypes == InputReader.ControlType.KeyboardMouse)
            {
                if (CurrentStep.icon.Count > 0)
                {
                    icon.rectTransform.rect.Set(CurrentStep.icon[1].rect.x, CurrentStep.icon[1].rect.y, CurrentStep.icon[1].rect.width, CurrentStep.icon[1].rect.height);
                    icon.enabled = true;
                    icon.sprite = CurrentStep.icon[1];
                }
                else
                    icon.enabled = false;
            }
        }

        public void ResetTutorial()
        {
            CurrentStepIndex = 0;
            CurrentStep = steps[CurrentStepIndex];
        }
        
        public void SetTutorialUI(bool isOn)
        {
            canvas.SetActive(isOn);
        }
    
        public void EndTutorial()
        {
            isFinished = true;
            canvas.SetActive(false);
        }

        public void LoadData(GameData data)
        {
            isFinished = data.tutoData.isTutoFinished;
            CurrentStepIndex = data.tutoData.currentTutoIndex;
            CurrentStep = steps[CurrentStepIndex];
        }

        public void SaveData(ref GameData data)
        {
            var tuto = new TutorialData()
            {
                isTutoFinished = isFinished,
                currentTutoIndex = CurrentStepIndex,
            };
            data.tutoData = tuto;
        }
    }
    
    [Serializable]
    public class TutorialStep {
        
        public string instruction;
        public List<Sprite> icon;
        public enum ActionType { Move, Jump, Dash, DoubleJump, LightAttack, HeavyAttack, Combo, Interact }
        public ActionType action;
    }
}