using System;
using System.Collections.Generic;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Localization;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.Tutorial;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Memoriam.Script.Plataformas
{
    public class InteractInstruction : MonoBehaviour, ILocalization
    {
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();
        [field: SerializeField] public List<LanguagesClass> languages = new List<LanguagesClass>();
        [SerializeField] private Image icon;
        [field: SerializeField] public TMP_Text TextToTranslateTMP { get; set; }
        [SerializeField] public CanvasGroup instructionsCanva;
        
        
        private void OnEnable()
        {
            Checkpoint.OnInteractReached += OnInteractCalled;
            CheckPointPlatform.OnInteractReached += OnInteractCalled;
            
        }

        private void OnDisable()
        {
            Checkpoint.OnInteractReached -= OnInteractCalled;
            CheckPointPlatform.OnInteractReached -= OnInteractCalled;
        }

        private void Update()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay) return;
            
            CheckForInput();
        }

        private void CheckForInput()
        {
            if (InputReader.Instance.ControlTypes == InputReader.ControlType.Control)
            {
                if (sprites.Count > 0)
                {
                    var sprite = sprites[0];
                    icon.sprite = sprite;
                    icon.rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
                    icon.rectTransform.sizeDelta *= 5;
                }
            }
            else if (InputReader.Instance.ControlTypes == InputReader.ControlType.KeyboardMouse)
            {
                if (sprites.Count > 0)
                {
                    var sprite = sprites[1];
                    icon.sprite = sprite;
                    icon.rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
                    icon.rectTransform.sizeDelta *= 5;
                }
            }
        }

        private void OnInteractCalled(bool enable)
        {
            instructionsCanva.alpha = enable ? 1 : 0;
        }

        public void Translate(Languages language)
        {
            foreach (var lang in languages)
            {
                if (lang.TryGetText(language, out var txt))
                {
                    TextToTranslateTMP.text = txt;
                    CheckForInput();
                    break;
                }
            }
        }
    }
}