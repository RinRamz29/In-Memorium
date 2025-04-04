using System;
using System.Collections.Generic;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    public class GameplayMenuManager : MonoBehaviour, ISaveableObject
    {
        [SerializeField] private GameObject pauseMenu;
        [field: SerializeField] public Slider HealthBar { get; private set; }

        [SerializeField] private List<Toggle> powerupToggles;
        private void ChangeHealthValue(float health) => HealthBar.value = health;

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
            Player.Player.OnPowerUpPickedUp += TogglePowerUp;
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            switch (GameStateManager.Instance.GameCurrentState)
            {
                case GameStateManager.GameState.OnPause:
                    GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnGameplay);
                    pauseMenu.SetActive(false);
                    break;
                case GameStateManager.GameState.OnGameplay:
                    GameStateManager.Instance.OnGameStateChanged?.Invoke(GameStateManager.GameState.OnPause);
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
        }

        public void LoadData(GameData data)
        {
            powerupToggles[0].isOn = data.gamePlayMenu.canDoubleJump;
            powerupToggles[1].isOn = data.gamePlayMenu.canDash;
        }

        public void SaveData(ref GameData data)
        {
            data.gamePlayMenu.canDoubleJump = powerupToggles[0].isOn;
            data.gamePlayMenu.canDash = powerupToggles[1].isOn;
        }
    }
}