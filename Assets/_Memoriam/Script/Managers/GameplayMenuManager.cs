using System;
using System.Collections.Generic;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Player;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    public class GameplayMenuManager : MonoBehaviour, ISaveableObject
    {
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject uiPlayer;
        [SerializeField] private Player.Player player;
        [field: SerializeField] public Slider HealthBar { get; private set; }
        [field: SerializeField] public Slider StaminaBar { get; private set; }
        [field: SerializeField] public Slider XpBar { get; private set; }
        [field: SerializeField] public TMP_Text LevelTxt { get; private set; }


        [SerializeField] private List<Toggle> powerupToggles;
        private void ChangeHealthValue(float health) => HealthBar.value = health;
        private void ChangeStaminaValue(float stamina) => StaminaBar.value = stamina;

        private void ChangeLvl(int lvl) => LevelTxt.text = "Lvl: " + lvl.ToString(format: "0.00");
        private void ChangeXp(float xp) => XpBar.value += (xp / player.Progression.XpToNextLevel);

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
            Player.Player.OnHealthChanged += ChangeHealthValue;
            Player.Player.OnStaminaChanged += ChangeStaminaValue;
            Player.Player.OnPowerUpPickedUp += TogglePowerUp;
            player.Progression.OnLevelUp += ChangeLvl;
            player.Progression.OnXpGained += ChangeXp;
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            switch (GameStateManager.Instance.GameCurrentState)
            {
                case GameStateManager.GameState.OnPause:
                    GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnGameplay);
                    uiPlayer.SetActive(true);
                    pauseMenu.SetActive(false);
                    break;
                case GameStateManager.GameState.OnGameplay:
                    GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnPause);
                    uiPlayer.SetActive(false);
                    pauseMenu.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDisable()
        {
            InputReader.Instance.PlayerActions.Player.Pause.performed -= OnPause;
            Player.Player.OnHealthChanged -= ChangeHealthValue;
            Player.Player.OnStaminaChanged -= ChangeStaminaValue;
        }

        public void LoadData(GameData data)
        {
            powerupToggles[0].isOn = data.gamePlayMenu.canDoubleJump;
            powerupToggles[1].isOn = data.gamePlayMenu.canDash;
            HealthBar.value = data.gamePlayMenu.health;
        }

        public void SaveData(ref GameData data)
        {
            data.gamePlayMenu.canDoubleJump = powerupToggles[0].isOn;
            data.gamePlayMenu.canDash = powerupToggles[1].isOn;
            data.gamePlayMenu.health = HealthBar.value;
        }
    }
}