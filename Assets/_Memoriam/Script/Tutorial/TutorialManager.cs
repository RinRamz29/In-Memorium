using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Memoriam.Script.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public List<TutorialStep> steps;
        public bool isFinished;
        public int CurrentStepIndex { get; private set; } = 0;
        public TutorialStep CurrentStep { get; private set; }
        //[SerializeField] private CanvasGroup uiGroup;
        //[SerializeField] private TextMeshProUGUI instructionText;

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
        
        public void NextStep()
        {
            CurrentStepIndex++;
            CurrentStep = steps[CurrentStepIndex];
            if (CurrentStepIndex < steps.Count) 
                ShowStep();
            else 
                EndTutorial();
        }
    
        public void ShowStep() {
            //instructionText.text = steps[CurrentStep].instruction;
            //uiGroup.alpha = 1;
        }
    
        public void EndTutorial()
        {
            isFinished = true;
            //uiGroup.alpha = 0;
        }
    }
    
    [Serializable]
    public class TutorialStep {
        
        public string instruction;
        public enum ActionType { Move, Jump, Dash, DoubleJump, LightAttack, HeavyAttack, ChargedAttack, Combo, EnterZone }
        public ActionType action;
    }
}