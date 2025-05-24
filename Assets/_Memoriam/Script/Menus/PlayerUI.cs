using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player;
using _Memoriam.Script.Powerups;
using UnityEngine;
using UnityEngine.UI;

namespace _Memoriam.Script.Menus
{
    public class PlayerUI : MonoBehaviour
    {
        private Player.Player _player;
        [SerializeField] private List<Sprite> icons;
        [SerializeField] private Image icon;
        [SerializeField] private Image timer;
        [SerializeField] private Image timerHolder;

        private void OnEnable()
        {
            Loader.Instance.OnSceneLoaded += Initialize;
            Player.Player.OnPowerUpPickedUp += CheckForAbility;
        }

        private void Initialize()
        {
            CheckForInput();

            _player = FindObjectsByType<Player.Player>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault();
            
            if (_player == null)
                return;
            
            CheckForAbility();
        }

        private void OnDisable()
        {
            Loader.Instance.OnSceneLoaded -= Initialize;
            Player.Player.OnPowerUpPickedUp -= CheckForAbility;
        }

        private void Update()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            if (_player == null)
                return;

            timer.fillAmount = _player.Timer;
            CheckForInput();
        }

        private void CheckForInput()
        {
            if (InputReader.Instance.ControlTypes == InputReader.ControlType.Control)
            {
                var sprite = icons[0];
                icon.sprite = sprite;
            }
            else if (InputReader.Instance.ControlTypes == InputReader.ControlType.KeyboardMouse)
            {
                var sprite = icons[1];
                icon.sprite = sprite;
            }
        }

        private void CheckForAbility()
        {
            if (!_player.abilities.hasDash)
            {
                icon.enabled = false;
                timer.enabled = false;
                timerHolder.enabled = false;
            }
            else
            {
                icon.enabled = true;
                timer.enabled = true;
                timerHolder.enabled = true;
            }
        }
        
        private void CheckForAbility(TypeOfPickable ability)
        {
            switch (ability)
            {
                case TypeOfPickable.Dash: 
                    icon.enabled = true;
                    timer.enabled = true;
                    timerHolder.enabled = true;
                    break;
                default:
                    icon.enabled = false;
                    timer.enabled = false;
                    timerHolder.enabled = false;
                    break;
            }
        }
    }
}