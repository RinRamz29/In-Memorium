using System;
using System.Collections.Generic;
using System.Linq;
using _Memoriam.Script.InputLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Memoriam.Script.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public List<TutorialStep> steps;
        public bool isFinished;
        public int CurrentStepIndex { get; private set; } = 0;
        public TutorialStep CurrentStep { get; private set; }
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject canvas;

        private void Awake()
        {
            if (steps == null || steps.Count == 0)
            {
                Debug.LogError("TutorialManager: no steps assigned!");
                enabled = false;
                return;
            }
            CurrentStep = steps[0];
        }

        private void Start()
        {
            ShowStep();
        }

        private void Update()
        {
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
    
        public void ShowStep() {

            CheckForInput();
            
            instructionText.text = CurrentStep.instruction;
            canvas.SetActive(true);
        }

        private void CheckForInput()
        {
            if (InputReader.Instance.ControlTypes == InputReader.ControlType.Control)
            {
                if (CurrentStep.icon.Count > 0)
                {
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
                    icon.enabled = true;
                    icon.sprite = CurrentStep.icon[1];
                }
                else
                    icon.enabled = false;
            }
        }
    
        public void EndTutorial()
        {
            isFinished = true;
            canvas.SetActive(false);
        }
    }
    
    [Serializable]
    public class TutorialStep {
        
        public string instruction;
        public List<Sprite> icon;
        public enum ActionType { Move, Jump, Dash, DoubleJump, LightAttack, HeavyAttack, ChargedAttack, Combo, Interact }
        public ActionType action;
    }
}