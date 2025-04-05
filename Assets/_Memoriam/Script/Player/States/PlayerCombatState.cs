using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player.VeilOfShadows.Hea.StateMachine;
using Unity.Cinemachine;
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

        private float _lightAttackCooldown = 1.4f;
        private float _heavyAttackCooldown = 1.75f;
        private float _lastAttackTime = -Mathf.Infinity;
        
        private float _lightAttackTimer;
        private float _heavyAttackTimer;
        

        private Color _cachedColor;

        private Vector2 _currentVelocity = Vector2.zero;
        private Vector3 _targetOffset;
        

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
            InputReader.Instance.PlayerActions.Player.ChargedLightAttack.performed +=
                _ => _player.ChargedInputReceived = true;
            InputReader.Instance.PlayerActions.Player.ChargedHeavyAttack.performed +=
                _ => _player.ChargedInputReceived = true;
            InputReader.Instance.PlayerActions.Player.LightCombo.performed += _ => _player.ComboInputReceived = true;
            InputReader.Instance.PlayerActions.Player.HeavyCombo.performed += _ => _player.ComboInputReceived = true;
            InputReader.Instance.PlayerActions.Player.Jump.performed += Jump;
            _cachedColor = _player.SpriteRenderer.color;
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
            
            _lightAttackTimer += Time.deltaTime;
            _heavyAttackTimer += Time.deltaTime;

            _player.heavyAttackBar.value = Mathf.Clamp01(_heavyAttackTimer / _heavyAttackCooldown);
            _player.lightAttackBar.value = Mathf.Clamp01(_lightAttackTimer / _lightAttackCooldown);
        }

        public void LateTick()
        {
            Move();
        }

        private void LightAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (Time.time - _lastAttackTime < _lightAttackCooldown)
            {
                return;
            }

            if (!_player.IsAttacking)
            {
                _player.IsAttacking = true;
                _damage = _player.Damage * 1f;
                _player.CurrentAttackType = Player.AttackType.Light;
                _lastAttackTime = Time.time;
                _lightAttackTimer = 0f;
                CheckForSwordCollisions();
                _player.Animator.SetBool(_player.LightAttackHash, true);
            }
        }

        private void HeavyAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (Time.time - _lastAttackTime < _heavyAttackCooldown)
            {
                return;
            }

            if (!_player.IsAttacking)
            {
                _player.IsAttacking = true;
                _damage = _player.Damage * 1.5f;
                _player.CurrentAttackType = Player.AttackType.Heavy;
                _lastAttackTime = Time.time;
                _heavyAttackTimer = 0f;
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
            _player.IsTouchingWall = false;

            foreach (var check in _player.WallCheck)
            {
                var direction = _isFlipped ? Vector2.left : Vector2.right;
                var hit = Physics2D.Raycast(check.position, direction, _player.WallCheckDistance, _player.GroundMask);
                Debug.DrawRay(check.position, direction * _player.WallCheckDistance, Color.red);

                if (hit.collider != null)
                {
                    _player.IsTouchingWall = true;

                    if (!_player.IsGrounded || _player.Rigidbody2D.linearVelocity.y > 0)
                        _player.IsGrounded = false;

                    break;
                }
            }

            float targetSpeedX = _player.Movement.x * _player.Speed;
            float smoothedX =
                Mathf.SmoothDamp(_player.Rigidbody2D.linearVelocity.x, targetSpeedX, ref _currentVelocity.x, 0.1f);
            float verticalSpeed = _player.Rigidbody2D.linearVelocity.y;

            if (_player.IsGrounded)
            {
                _player.Rigidbody2D.linearVelocity = new Vector2(smoothedX, verticalSpeed);
                _player.Animator.SetFloat(_player.SpeedXHash, Mathf.Abs(_player.Movement.x));
            }
            else if (_player.IsTouchingWall && !_player.IsGrounded)
            {
                verticalSpeed = Mathf.Max(_player.Rigidbody2D.linearVelocity.y, -_player.WallSlideSpeed);
                _player.Rigidbody2D.linearVelocity = new Vector2(0f, verticalSpeed);
            }
            else
            {
                float airSpeedX = Mathf.SmoothDamp(_player.Rigidbody2D.linearVelocity.x,
                    _player.Movement.x * AirControlMultiplier * _player.Speed, ref _currentVelocity.x, 0.15f);
                _player.Rigidbody2D.linearVelocity = new Vector2(airSpeedX, verticalSpeed);
                _player.Animator.SetFloat(_player.SpeedXHash, 0);
            }

            if (_isDashing)
            {
                _player.Rigidbody2D.linearVelocity =
                    new Vector2(_player.Rigidbody2D.linearVelocity.x, _player.Rigidbody2D.linearVelocity.y);
                _direction = _isFlipped ? Vector2.left : Vector2.right;
                _player.Rigidbody2D.AddForce(_direction * _player.DashForce, ForceMode2D.Impulse);

                if ((_direction.x < -0.1f && _player.Movement.x > 0.1f) ||
                    (_direction.x > 0.1f && _player.Movement.x < -0.1f))
                {
                    _isDashing = false;
                    _player.CanDash = false;
                }

                _dashCooldown -= Time.deltaTime;
                if (_dashCooldown <= 0)
                {
                    _isDashing = false;
                    _player.CanDash = false;
                }
            }

            if (!_player.CanDoubleJump && _player.IsGrounded && _player.DoubleJumpPickedUp)
            {
                _player.CanDoubleJump = true;
            }

            if (!_player.CanDash && _player.IsGrounded && _player.DashPickedUp)
            {
                _player.CanDash = true;
            }

            UpdateCameraOffset();
            
            var moveX = _player.Movement.normalized.x;

            // Horizontal offset based on direction
            if (moveX > 0.1f)
            {
                _player.SpriteRenderer.flipX = false;
                _isFlipped = false;
            }
            else if (moveX < -0.1f)
            {
                _player.SpriteRenderer.flipX = true;
                _isFlipped = true;
            }

            float yVelocity = _player.Rigidbody2D.linearVelocity.y;
            if (yVelocity > 0.1f)
                _player.Animator.SetFloat(_player.SpeedYHash, 1);
            else if (yVelocity < -0.1f)
                _player.Animator.SetFloat(_player.SpeedYHash, -1);
            else
                _player.Animator.SetFloat(_player.SpeedYHash, 0);
        }
        
        private void UpdateCameraOffset()
        {
            var moveX = _player.Movement.x;

            switch (moveX)
            {
                // Horizontal offset based on direction
                case > 0.9f:
                    _targetOffset.x = _player.CameraHorizontalOffset;
                    break;
                case < -0.8f:
                    _targetOffset.x = -_player.CameraHorizontalOffset;
                    break;
            }

            _targetOffset.y = _player.CinemachineFollow.FollowOffset.y;
            _targetOffset.z = _player.CinemachineFollow.FollowOffset.z;
            
            // Lerp for smooth transition
            Vector3 currentOffset =  _player.CinemachineFollow.FollowOffset;
            _player.CinemachineFollow.FollowOffset = Vector3.Lerp(currentOffset, _targetOffset, Time.deltaTime * _player.CameraOffsetLerpSpeed);
        }


        private void Jump(InputAction.CallbackContext context)
        {
            if (!context.performed ||
                GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            if (context.performed && _player.IsGrounded)
                _player.Rigidbody2D.AddForce(Vector2.up * _player.JumpForce, ForceMode2D.Impulse);

            if (!context.performed || _player.IsGrounded || !_player.CanDoubleJump)
                return;

            _player.CanDoubleJump = false;
            // Reset vertical velocity before double jump
            _player.Rigidbody2D.linearVelocity = new Vector2(_player.Rigidbody2D.linearVelocity.x, 0f);
            _player.Rigidbody2D.AddForce(Vector2.up * (_player.JumpForce * 1.1f), ForceMode2D.Impulse);
        }

        private void Dash(InputAction.CallbackContext context)
        {
            if (!context.performed || !_player.CanDash)
                return;

            _isDashing = true;
            _dashCooldown = 0.45f;
        }

        private void CheckForSwordCollisions()
        {
            var sizeOfCapsule = _isFlipped ? new Vector2(-2.5f, 1f) : new Vector2(2.5f, 1.0f);

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