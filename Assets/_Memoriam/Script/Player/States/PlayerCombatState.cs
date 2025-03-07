using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player.VeilOfShadows.Hea.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Memoriam.Script.Player.States
{
    public class PlayerCombatState : IState
    {
        private Player _player;
        private float _damage;
        private bool _isFlipped;
        private Vector2 _direction;

        private bool _isDashing;
        private float _dashCooldown;
        private const float AirControlMultiplier = 0.75f;


        public PlayerCombatState(Player player)
        {
            _player = player;
            _player.CurrentAttackType = Player.AttackType.None;
        }

        public void Enter()
        {
            InputReader.Instance.PlayerActions.Player.LightAttack.performed += LightAttack;
            InputReader.Instance.PlayerActions.Player.HeavyAttack.performed += HeavyAttack;
            InputReader.Instance.PlayerActions.Player.Dash.performed += Dash;
            InputReader.Instance.PlayerActions.Player.ChargedLightAttack.performed += _ => _player.ChargedInputReceived = true;
            InputReader.Instance.PlayerActions.Player.ChargedHeavyAttack.performed += _ => _player.ChargedInputReceived = true;
            InputReader.Instance.PlayerActions.Player.LightCombo.performed += _ => _player.ComboInputReceived = true;
            InputReader.Instance.PlayerActions.Player.HeavyCombo.performed += _ => _player.ComboInputReceived = true;
            InputReader.Instance.PlayerActions.Player.Jump.performed += Jump;


            _damage = _player.Damage;
        }

        public void Exit()
        {
            InputReader.Instance.PlayerActions.Player.LightAttack.performed -= LightAttack;
            InputReader.Instance.PlayerActions.Player.HeavyAttack.performed -= HeavyAttack;
            InputReader.Instance.PlayerActions.Player.Dash.performed -= Dash;
            InputReader.Instance.PlayerActions.Player.Jump.performed -= Jump;

            // Reset attack state
            _player.IsAttacking = false;
            _player.ComboWindowOpen = false;
            _player.ComboInputReceived = false;
            _player.CurrentAttackType = Player.AttackType.None;
        }

        public void Tick()
        {
            // Check if we need to execute the combo
            if (_player.ComboInputReceived && _player.ComboWindowOpen)
            {
                ExecuteCombo();
            }
            else if (_player.ChargedInputReceived && _player.ComboWindowOpen)
            { 
                ExecuteChargedAttack();
            }
        }

        public void LateTick()
        {
            Move();
        }

        private void LightAttack(InputAction.CallbackContext context)
        {
            if (!context.performed ||
                GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            if (!_player.IsAttacking)
            {
                // First light attack
                _player.IsAttacking = true;
                _damage = _player.Damage * 1f;
                _player.CurrentAttackType = Player.AttackType.Light;
                CheckForSwordCollisions();
                _player.Animator.SetBool(_player.LightAttackHash, true);
            }
        }

        private void HeavyAttack(InputAction.CallbackContext context)
        {
            if (!context.performed ||
                GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            if (!_player.IsAttacking)
            {
                // First heavy attack
                _player.IsAttacking = true;
                _damage = _player.Damage * 1.5f;
                _player.CurrentAttackType = Player.AttackType.Heavy;
                CheckForSwordCollisions();
                _player.Animator.SetBool(_player.HeavyAttackHash, true);
            }
        }

        private void ExecuteChargedAttack()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            _player.ChargedInputReceived = false;
            _player.ComboWindowOpen = false;
            
            CheckForSwordCollisions();
            _player.Animator.SetBool(_player.ComboTriggeredHash, true);

            if (_player.CurrentAttackType == Player.AttackType.Light)
            {
                _damage = _player.Damage * 1.6f;
                _player.Animator.SetBool(_player.LightAttackHash, false);
                _player.Animator.SetTrigger(_player.ChargedTriggeredHash);
            }
            else if (_player.CurrentAttackType == Player.AttackType.Heavy)
            {
                _damage = _player.Damage * 2f;
                _player.Animator.SetBool(_player.HeavyAttackHash, false);
                _player.Animator.SetTrigger(_player.ChargedHeavyTriggeredHash);
            }
        }

        private void ExecuteCombo()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            _player.ComboInputReceived = false;
            _player.ComboWindowOpen = false;

            CheckForSwordCollisions();
            _player.Animator.SetBool(_player.ComboTriggeredHash, true);

            if (_player.CurrentAttackType == Player.AttackType.Light)
            {
                _damage = _player.Damage * 1.4f;
                _player.Animator.SetBool(_player.LightAttackHash, false);
                _player.Animator.SetTrigger(_player.Combo1AttackHash);
            }
            else if (_player.CurrentAttackType == Player.AttackType.Heavy)
            {
                _damage = _player.Damage * 1.7f;
                _player.Animator.SetBool(_player.HeavyAttackHash, false);
                _player.Animator.SetTrigger(_player.Combo2AttackHash);
            }
        }

        private void Move()
        {
            _player.Movement = InputReader.Instance.PlayerActions.Player.Move.ReadValue<Vector2>();

            _player.IsGrounded =
                Physics2D.OverlapCircle(_player.GroundCheck.position, _player.GroundDistance, _player.GroundMask);
            
            if (_player.IsGrounded)
            {
                _player.Rigidbody2D.linearVelocity =
                    new Vector2(_player.Movement.x * _player.Speed, _player.Rigidbody2D.linearVelocity.y);
                _player.Animator.SetFloat(_player.SpeedXHash, _player.Movement.x);
            }
            else
            {
                _player.Rigidbody2D.linearVelocity =
                    new Vector2((_player.Movement.x * AirControlMultiplier) * _player.Speed, _player.Rigidbody2D.linearVelocity.y);
                _player.Animator.SetFloat(_player.SpeedXHash, 0);
            }
            
            if (_isDashing)
            {
                _player.Rigidbody2D.linearVelocity =
                    new Vector2(_player.Rigidbody2D.linearVelocity.y, _player.Rigidbody2D.linearVelocity.y);
                _direction = _isFlipped ? Vector2.left : Vector2.right;
                _player.Rigidbody2D.AddForce(_direction * _player.DashForce, ForceMode2D.Impulse);

                _dashCooldown -= Time.deltaTime;
                if (_dashCooldown <= 0)
                {
                    _isDashing = false;
                    _player.CanDash = false;
                }
            }

            if (!_player.CanDoubleJump)
            {
                if (_player.IsGrounded && _player.DoubleJumpPickedUp)
                {
                    _player.CanDoubleJump = true;
                }
            }
            
            if (!_player.CanDash)
            {
                if (_player.IsGrounded && _player.DashPickedUp)
                {
                    _player.CanDash = true;
                }
            }
            

            switch (_player.Movement.normalized.x)
            {
                case > 0.1f:
                    _player.SpriteRenderer.flipX = false;
                    _isFlipped = false;
                    break;
                case < -0.1f:
                    _player.SpriteRenderer.flipX = true;
                    _isFlipped = true;
                    break;
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

            if (context.performed && _player.IsGrounded)
                _player.Rigidbody2D.AddForce(Vector2.up * _player.JumpForce, ForceMode2D.Impulse);

            if (!context.performed || _player.IsGrounded || !_player.CanDoubleJump)
                return;

            _player.CanDoubleJump = false;
            _player.Rigidbody2D.AddForce(Vector2.up * _player.JumpForce, ForceMode2D.Impulse);
        }

        private void Dash(InputAction.CallbackContext context)
        {
            if (!context.performed || !_player.CanDash)
                return;

            _isDashing = true;
            _dashCooldown = 0.2f;
        }

        private void CheckForSwordCollisions()
        {
            var sizeOfCapsule = _isFlipped ? new Vector2(-2.0f, 1f) : new Vector2(2.0f, 1.0f);

            _player.SwordCollider.transform.localPosition = _isFlipped
                ? new Vector3(-1f, _player.SwordCollider.transform.localPosition.y,
                    _player.SwordCollider.transform.localPosition.z)
                : new Vector3(1f, _player.SwordCollider.transform.localPosition.y,
                    _player.SwordCollider.transform.localPosition.z);

            var results = Physics2D.OverlapCapsuleAll(_player.SwordCollider.transform.position, sizeOfCapsule,
                CapsuleDirection2D.Horizontal, _player.EnemyLayer);

            foreach (var result in results)
            {
                if (result.TryGetComponent<IEnemy>(out var enemy))
                {
                    enemy.ReceiveDamage(_damage);
                }
            }
        }
    }
}