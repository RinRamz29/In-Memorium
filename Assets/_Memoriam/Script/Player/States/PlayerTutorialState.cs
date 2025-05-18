using System;
using System.Collections;
using _Memoriam.Script.Audio;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.Tutorial;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace _Memoriam.Script.Player.States
{
    public class PlayerTutorialState : IState
    {
        private Player _player;

        public PlayerTutorialState(Player player)
        {
            _player = player;
            _player.CurrentAttackType = Player.AttackType.None;
        }

        #region UnityFlow

        public void Enter()
        {
            Debug.Log("Enter to Tutorial State");
            TutorialManager.OnTutorialLoaded += OnTutorialLoaded;
            Player.OnPowerUpPickedUp += PowerUpPickedUp;
            Player.onPlayerFirstTp += ReachedFirstTeleport;
            InputReader.Instance.PlayerActions.Player.Jump.performed += _player.Jump;
            InputReader.Instance.PlayerActions.Player.LightAttack.performed += _player.LightAttack;
            InputReader.Instance.PlayerActions.Player.HeavyAttack.performed += _player.HeavyAttack;
            InputReader.Instance.PlayerActions.Player.LightCombo.performed += _ => _player.ComboInputReceived = true;
            InputReader.Instance.PlayerActions.Player.HeavyCombo.performed += _ => _player.ComboInputReceived = true;
            InputReader.Instance.PlayerActions.Player.Dash.performed += _player.Dash;
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

        public void Exit()
        {
            Unsubscribe();

            InputReader.Instance.PlayerActions.Player.LightAttack.performed -= _player.LightAttack;
            InputReader.Instance.PlayerActions.Player.HeavyAttack.performed -= _player.HeavyAttack;
            InputReader.Instance.PlayerActions.Player.Dash.performed -= _player.Dash;
            InputReader.Instance.PlayerActions.Player.Jump.performed -= _player.Jump;
            Player.OnPowerUpPickedUp -= PowerUpPickedUp;
            Player.onPlayerFirstTp -= ReachedFirstTeleport;

            // Reset attack state
            _player.IsAttacking = false;
            _player.ComboWindowOpen = false;
            _player.ComboInputReceived = false;
            _player.CurrentAttackType = Player.AttackType.None;
        }

        #endregion

        #region TutoLogic

        private void OnTutorialLoaded(TutorialStep step)
        {
            if (TutorialManager.Instance.CurrentStepIndex + 1 > TutorialManager.Instance.steps.Count - 1)
            {
                TutorialManager.Instance.EndTutorial();
                _player.StateMachine.ChangeState(new PlayerCombatState(_player));
                return;
            }
            
            Subscribe(step);
            TutorialManager.OnTutorialLoaded -= OnTutorialLoaded;
        }

        private void Subscribe(TutorialStep step)
        {
            Debug.Log("Subscribed");
            if (step.action == TutorialStep.ActionType.Jump)
            {
                Unsubscribe();
                InputReader.Instance.PlayerActions.Player.Jump.performed += OnStepCompleted;
                return;
            }

            if (step.action == TutorialStep.ActionType.LightAttack)
            {
                Unsubscribe();
                InputReader.Instance.PlayerActions.Player.LightAttack.performed += OnStepCompleted;
                return;
            }

            if (step.action == TutorialStep.ActionType.HeavyAttack)
            {
                Unsubscribe();
                InputReader.Instance.PlayerActions.Player.HeavyAttack.performed += OnStepCompleted;
                return;
            }

            if (step.action == TutorialStep.ActionType.Combo)
            {
                Unsubscribe();
                InputReader.Instance.PlayerActions.Player.LightCombo.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.HeavyCombo.performed += OnStepCompleted;
                return;
            }

            if (step.action == TutorialStep.ActionType.Interact)
            {
                Unsubscribe();
                InputReader.Instance.PlayerActions.Player.Interact.performed += OnStepCompleted;
                return;
            }

            if (step.action == TutorialStep.ActionType.DoubleJump)
            {
                Unsubscribe();
                InputReader.Instance.PlayerActions.Player.Jump.performed += OnDoubleJump;
                return;
            }

            if (step.action == TutorialStep.ActionType.Dash)
            {
                Unsubscribe();
                InputReader.Instance.PlayerActions.Player.Dash.performed += OnDash;
                return;
            }
        }

        private void OnStepCompleted(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            if (TutorialManager.Instance.CurrentStepIndex + 1 > TutorialManager.Instance.steps.Count - 1)
            {
                TutorialManager.Instance.EndTutorial();
                _player.StateMachine.ChangeState(new PlayerCombatState(_player));
                return;
            }

            var next = TutorialManager.Instance.steps[TutorialManager.Instance.CurrentStepIndex + 1];

            if (next.action == TutorialStep.ActionType.DoubleJump ||
                next.action == TutorialStep.ActionType.Interact ||
                next.action == TutorialStep.ActionType.Dash)
            {
                TutorialManager.Instance.TutoActive = false;
                TutorialManager.Instance.SetCanvas(false);
            }

            AdvanceIfCorrect();
        }

        private void PowerUpPickedUp(TypeOfPickable powerUpType)
        {
            if (powerUpType != TypeOfPickable.DoubleJump && powerUpType != TypeOfPickable.Dash)
                return;

            TutorialManager.Instance.SetCanvas(true);
            TutorialManager.Instance.TutoActive = true;
        }

        private void ReachedFirstTeleport(bool condition)
        {
            if (TutorialManager.Instance.steps[TutorialManager.Instance.CurrentStepIndex].action ==
                TutorialStep.ActionType.Interact && condition)
            {
                TutorialManager.Instance.SetCanvas(true);
                TutorialManager.Instance.TutoActive = true;
            }
        }

        private void OnDoubleJump(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !_player.canDoubleJump)
                return;

            TutorialManager.Instance.SetCanvas(false);
            TutorialManager.Instance.TutoActive = false;
            AdvanceIfCorrect();
        }

        private void OnDash(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !_player.canDash)
                return;

            TutorialManager.Instance.SetCanvas(false);
            TutorialManager.Instance.TutoActive = false;
            AdvanceIfCorrect();
        }

        private void AdvanceIfCorrect()
        {
            if (TutorialManager.Instance.CurrentStepIndex + 1 > TutorialManager.Instance.steps.Count - 1)
            {
                TutorialManager.Instance.EndTutorial();
                _player.StateMachine.ChangeState(new PlayerCombatState(_player));
                return;
            }
            
            TutorialManager.Instance.NextStep();
            Subscribe(TutorialManager.Instance.steps[TutorialManager.Instance.CurrentStepIndex]);
        }

        private void Unsubscribe()
        {            
            TutorialManager.OnTutorialLoaded -= OnTutorialLoaded;
            InputReader.Instance.PlayerActions.Player.Jump.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.Jump.performed -= OnDoubleJump;
            InputReader.Instance.PlayerActions.Player.Dash.performed -= OnDash;
            InputReader.Instance.PlayerActions.Player.LightAttack.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.HeavyAttack.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.LightCombo.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.HeavyCombo.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.Interact.performed -= OnStepCompleted;
        }

        #endregion
    }
}