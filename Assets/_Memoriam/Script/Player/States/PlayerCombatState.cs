using System.Collections;
using _Memoriam.Script.Audio;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Memoriam.Script.Player.States
{
    public class PlayerCombatState : IState
    {
        private Player _player;

        public PlayerCombatState(Player player)
        {
            _player = player;
            _player.CurrentAttackType = Player.AttackType.None;
        }

        #region UnityFlow

        public void Enter()
        {
            Debug.Log("Enter to Combat State");
            InputReader.Instance.PlayerActions.Player.LightAttack.performed += _player.LightAttack;
            InputReader.Instance.PlayerActions.Player.HeavyAttack.performed += _player.HeavyAttack;
            InputReader.Instance.PlayerActions.Player.Dash.performed += _player.Dash;
            InputReader.Instance.PlayerActions.Player.LightCombo.performed += _ => _player.ComboInputReceived = true;
            InputReader.Instance.PlayerActions.Player.HeavyCombo.performed += _ => _player.ComboInputReceived = true;
            InputReader.Instance.PlayerActions.Player.Jump.performed += _player.Jump;
        }

        public void Exit()
        {
            InputReader.Instance.PlayerActions.Player.LightAttack.performed -= _player.LightAttack;
            InputReader.Instance.PlayerActions.Player.HeavyAttack.performed -= _player.HeavyAttack;
            InputReader.Instance.PlayerActions.Player.Dash.performed -= _player.Dash;
            InputReader.Instance.PlayerActions.Player.Jump.performed -= _player.Jump;

            // Reset attack state
            _player.IsAttacking = false;
            _player.ComboWindowOpen = false;
            _player.ComboInputReceived = false;
            _player.CurrentAttackType = Player.AttackType.None;
        }

        public void Tick()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            // Check if we need to execute the combo
            if (_player.ComboInputReceived && _player.ComboWindowOpen)
            {
                _player.ExecuteCombo();
            }

            if (!_player.IsAttacking && Time.time - _player.LastStaminaUseTime > _player.StaminaRegenDelay)
            {
                _player.Stamina += _player.StaminaRegenRate * Time.deltaTime;
                _player.Stamina = Mathf.Clamp(_player.Stamina, 0, _player.MaxStamina);
                Player.OnStaminaChanged?.Invoke(_player.Stamina / _player.MaxStamina);
            }
        }

        public void LateTick()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            _player.Move();
        }


        #endregion
    }
}