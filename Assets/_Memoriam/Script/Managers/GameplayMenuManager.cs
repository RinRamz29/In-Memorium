using System;
using System.Collections;
using System.Collections.Generic;
using _Memoriam.Script.Audio;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Player;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    public class GameplayMenuManager : MonoBehaviour, ISaveableObject
    {
        [SerializeField] private GameObject uiPlayer;
        [field: SerializeField] public Slider HealthBar { get; private set; }
        [field: SerializeField] public Slider StaminaBar { get; private set; }
        [field: SerializeField] public Slider XpBar { get; private set; }
        [field: SerializeField] public TMP_Text LevelTxt { get; private set; }
        
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject tutoUI;
        [SerializeField] private GameObject firstButton;
        [SerializeField] private SceneDataBase sceneData;


        [SerializeField] private List<Toggle> powerupToggles;
        private void ChangeHealthValue(float health) => HealthBar.value = health;
        private void ChangeStaminaValue(float stamina) => StaminaBar.value = stamina;
        
        public Player.Player Player { get; set; }

        private void ChangeLvl(int lvl) => LevelTxt.text = "Lvl: " + lvl.ToString(format: "0");
        private void ChangeXp(float xp)
        {
            float normalized = Player.Progression.CurrentXp /
                               Player.Progression.XpToNextLevel;

            XpBar.value = normalized;
        }

        private void TogglePowerUp(TypeOfPickable pickable)
        {
            switch (pickable)
            {
                case TypeOfPickable.DoubleJump:
                    powerupToggles[0].isOn = true;
                    break;
                case TypeOfPickable.Dash:
                    powerupToggles[1].isOn = true;
                    break;
            }
        }

        private void Awake()
        {
            InputReader.Instance.PlayerActions.Player.Pause.performed += OnPause;
            Script.Player.Player.OnHealthChanged += ChangeHealthValue;
            Script.Player.Player.OnStaminaChanged += ChangeStaminaValue;
            Script.Player.Player.OnPowerUpPickedUp += TogglePowerUp; 
            PlayerProgression.OnLevelUp += ChangeLvl;
            PlayerProgression.OnXpGained += ChangeXp;
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            switch (GameStateManager.Instance.GameCurrentState)
            {
                case GameStateManager.GameState.OnPause:
                    Resume();
                    break;
                case GameStateManager.GameState.OnGameplay:
                    Pause();
                    break;
            }
        }
        
        public void Resume()
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonSelectSFX");
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnGameplay);
            uiPlayer.SetActive(true);
            pauseMenu.SetActive(false);
            tutoUI.SetActive(true);
        }

        private void Pause()
        {
            EventSystem.current.SetSelectedGameObject(firstButton);
            GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnPause);
            uiPlayer.SetActive(false);
            pauseMenu.SetActive(true);
            tutoUI.SetActive(false);
        }
        
        public async void Menu()
        {
            await Loader.Instance.LoadLoader(false);
        }

        public void Quit()
        {
            Application.Quit();
        }
        
        private void OnDisable()
        {
            InputReader.Instance.PlayerActions.Player.Pause.performed -= OnPause;
            Script.Player.Player.OnHealthChanged -= ChangeHealthValue;
            Script.Player.Player.OnStaminaChanged -= ChangeStaminaValue;
        }

        public void LoadData(GameData data)
        {
            powerupToggles[0].isOn = data.player.abilities.hasDoubleJump;
            powerupToggles[1].isOn = data.player.abilities.hasDash;
            HealthBar.value = data.player.health;
            LevelTxt.text = "Lvl: " + data.player.level.ToString("0");
            XpBar.value = data.player.xp;
        }

        public void SaveData(ref GameData data)
        {
        }
    }
}