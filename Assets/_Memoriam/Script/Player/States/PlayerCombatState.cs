using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player.VeilOfShadows.Hea.StateMachine;
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

        public void Enter()
        {
            _player.PlayerActions.Player.LightAttack.performed += LightAttack;
            _player.PlayerActions.Player.HeavyAttack.performed += HeavyAttack;
            _player.PlayerActions.Player.LightCombo.performed += _ => _player.ComboInputReceived = true;
            _player.PlayerActions.Player.HeavyCombo.performed += _ => _player.ComboInputReceived = true;
            _player.PlayerActions.Player.Jump.performed += Jump;
        }

        public void Exit()
        {
            _player.PlayerActions.Player.LightAttack.performed -= LightAttack;
            _player.PlayerActions.Player.HeavyAttack.performed -= HeavyAttack;
            _player.PlayerActions.Player.Jump.performed -= Jump;
            
            // Reset attack state
            _player.IsAttacking = false;
            _player.ComboWindowOpen = false;
            _player.ComboInputReceived = false;
            _player.CurrentAttackType = Player.AttackType.None;
        }

        public void Tick()
        {
            Move();
            
            // Check if we need to execute the combo
            if (_player.ComboInputReceived && _player.ComboWindowOpen)
            {
                ExecuteCombo();
            }
        }

        public void LateTick()
        {
        }

        private void LightAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            if (!_player.IsAttacking)
            {
                // First light attack
                _player.IsAttacking = true;
                _player.CurrentAttackType = Player.AttackType.Light;
                CheckForSwordCollisions();
                _player.Animator.SetBool(_player.LightAttackHash, true);
            }
        }
        
        private void HeavyAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            if (!_player.IsAttacking)
            {
                // First heavy attack
                _player.IsAttacking = true;
                _player.CurrentAttackType = Player.AttackType.Heavy;
                CheckForSwordCollisions();
                _player.Animator.SetBool(_player.HeavyAttackHash, true);
            }
        }

        private void ExecuteCombo()
        {
            _player.ComboInputReceived = false;
            _player.ComboWindowOpen  = false;
            
            CheckForSwordCollisions();
            Debug.Log("Combo");
            _player.Animator.SetBool(_player.ComboTriggeredHash, true);
            
            if (_player.CurrentAttackType == Player.AttackType.Light)
            {
                _player.Animator.SetBool(_player.LightAttackHash, false);
                _player.Animator.SetTrigger(_player.Combo1AttackHash);
            }
            else if (_player.CurrentAttackType == Player.AttackType.Heavy)
            {
                _player.Animator.SetBool(_player.HeavyAttackHash, false);
                _player.Animator.SetTrigger(_player.Combo2AttackHash);
            }
        }

        private void Move()
        {
            _player.Movement = _player.PlayerActions.Player.Move.ReadValue<Vector2>();
            _player.Rigidbody2D.linearVelocity = new Vector2(_player.Movement.x * _player.Speed, _player.Rigidbody2D.linearVelocity.y);

            _player.IsGrounded = Physics2D.OverlapCircle(_player.GroundCheck.position, _player.GroundDistance, _player.GroundMask);

            switch (_player.Movement.normalized.x)
            {
                case > 0.1f:
                    _player.SpriteRenderer.flipX = false; 
                    break;
                case < -0.1f:
                    _player.SpriteRenderer.flipX = true; 
                    break;
            }

            if (_player.IsGrounded)
            {
                _player.Animator.SetFloat(_player.SpeedXHash, _player.Movement.x);
            }
            else
            {
                _player.Animator.SetFloat(_player.SpeedXHash, 0);
            }

            switch (_player.Rigidbody2D.linearVelocity.y)
            {
                case > 0.1f:
                    _player.Animator.SetFloat(_player.SpeedYHash, 1);
                    break;
                case < -0.1f:
                    _player.Animator.SetFloat(_player.SpeedYHash, -1);
                    break;
                default:
                    _player.Animator.SetFloat(_player.SpeedYHash, 0);
                    break;
            }
        }

        private void Jump(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (!_player.IsGrounded) 
                return;
            
            _player.Rigidbody2D.AddForce(Vector2.up * _player.JumpForce, ForceMode2D.Impulse);
        }

        private void CheckForSwordCollisions()
        {
            var results = Physics2D.OverlapCircleAll(_player.SwordCollider.transform.position, 2.0f, _player.EnemyLayer);
            
            foreach (var result in results)
            {
                if (result.TryGetComponent<IEnemy>(out var enemy))
                {
                    enemy.ReceiveDamage(_player.Damage);
                }
            }
        }
    }
}