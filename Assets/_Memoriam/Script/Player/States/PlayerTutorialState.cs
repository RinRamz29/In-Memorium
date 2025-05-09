using System;
using System.Collections;
using _Memoriam.Script.Audio;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player.VeilOfShadows.Hea.StateMachine;
using _Memoriam.Script.Tutorial;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Memoriam.Script.Player.States
{
    public class PlayerTutorialState : IState
    {
        private Player _player;
        private TutorialManager _tutorial;
        
        private bool _isFlipped;
        private Vector2 _direction;

        private bool _isDashing;
        private float _dashCooldown;
        private const float AirControlMultiplier = 0.75f;


        private float _lastStaminaUseTime = -Mathf.Infinity;
        private float _staminaRegenDelay = 2f; // seconds after last use
        private float _staminaRegenRate = 15f; // stamina per second
        
        private float _counter;
        private Vector2 _currentVelocity = Vector2.zero;
        private Vector3 _targetOffset;

        public PlayerTutorialState(Player player, TutorialManager tutorial)
        {
            _player = player;
            _player.CurrentAttackType = Player.AttackType.None;
            _tutorial = tutorial;
        }

        public void Enter()
        {
            var step = _tutorial.CurrentStep;
            Subscribe(step);
        }

        public void Exit()
        {
            Unsubscribe();   
            
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

        private void Subscribe(TutorialStep step)
        {
            if (step.action == TutorialStep.ActionType.Move)
            {
                InputReader.Instance.PlayerActions.Player.Move.performed += OnStepCompleted;
            }

            if (step.action == TutorialStep.ActionType.Jump)
            {
                InputReader.Instance.PlayerActions.Player.Jump.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.Jump.performed += Jump;
            }
            
            if (step.action == TutorialStep.ActionType.LightAttack)
            {
                InputReader.Instance.PlayerActions.Player.LightAttack.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.LightAttack.performed += LightAttack;
            }
            
            if (step.action == TutorialStep.ActionType.HeavyAttack)
            {
                InputReader.Instance.PlayerActions.Player.HeavyAttack.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.HeavyAttack.performed += HeavyAttack;
            }
            
            if (step.action == TutorialStep.ActionType.ChargedAttack)
            {
                InputReader.Instance.PlayerActions.Player.ChargedLightAttack.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.ChargedHeavyAttack.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.ChargedLightAttack.performed +=
                    _ => _player.ChargedInputReceived = true;
                InputReader.Instance.PlayerActions.Player.ChargedHeavyAttack.performed +=
                    _ => _player.ChargedInputReceived = true;
            }
            
            if (step.action == TutorialStep.ActionType.Combo)
            {
                InputReader.Instance.PlayerActions.Player.LightCombo.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.HeavyCombo.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.LightCombo.performed += _ => _player.ComboInputReceived = true;
                InputReader.Instance.PlayerActions.Player.HeavyCombo.performed += _ => _player.ComboInputReceived = true;
            }
                        
            if (step.action == TutorialStep.ActionType.DoubleJump)
            {
                InputReader.Instance.PlayerActions.Player.Jump.performed += OnDoubleJump;
            }
            
            if (step.action == TutorialStep.ActionType.Dash)
            {
                InputReader.Instance.PlayerActions.Player.Dash.performed += OnStepCompleted;
                InputReader.Instance.PlayerActions.Player.Dash.performed += Dash;
            }
        }

        private void OnStepCompleted(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;
            
            AdvanceIfCorrect();
        }
        
        private void OnDoubleJump(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && !_player.IsGrounded && _player.canDoubleJump)
            {
                InputReader.Instance.PlayerActions.Player.Jump.performed -= OnDoubleJump;
                AdvanceIfCorrect();
            }
        }

        private void Unsubscribe()
        {
            InputReader.Instance.PlayerActions.Player.Move.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.Jump.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.LightAttack.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.HeavyAttack.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.ChargedLightAttack.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.ChargedHeavyAttack.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.LightCombo.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.HeavyCombo.performed -= OnStepCompleted;
            InputReader.Instance.PlayerActions.Player.Dash.performed -= OnStepCompleted;
        }
        
        private void AdvanceIfCorrect()
        {
            Unsubscribe();             
            _tutorial.NextStep();   
            var step = _tutorial.CurrentStep;
            Debug.Log("Step completado, avanzando al siguiente");
            if (!_tutorial.isFinished)
            {
                Subscribe(step);   
                _tutorial.ShowStep();
            }
            else
            {
                _player.StateMachine.ChangeState(new PlayerCombatState(_player));
            }
        }

        #region UnityFlow

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
            
            if (!_player.IsAttacking && Time.time - _lastStaminaUseTime > _staminaRegenDelay)
            {
                _player.Stamina += _staminaRegenRate * Time.deltaTime;
                _player.Stamina = Mathf.Clamp(_player.Stamina, 0, _player.MaxStamina);
                Player.OnStaminaChanged?.Invoke(_player.Stamina / _player.MaxStamina);
            }
        }

        public void LateTick()
        {
            Move();
        }

        #endregion
        
        #region AttackLogic
        
        private void LightAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (_player.Stamina < _player.LighAttackStamina)
            {
                return;
            }

            if (!_player.IsAttacking)
            {
                _player.IsAttacking = true;
                _player.SetAttack(Player.AttackStrength.Light);

                _player.Stamina -= _player.LighAttackStamina;
                Player.OnStaminaChanged?.Invoke(_player.Stamina / _player.MaxStamina);
                _lastStaminaUseTime = Time.time;
                CheckForSwordCollisions();
                _player.Animator.SetBool(_player.LightAttackHash, true);
            }
        }

        private void HeavyAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (_player.Stamina < _player.HeavyAttackStamina)
            {
                return;
            }

            if (!_player.IsAttacking)
            {
                _player.IsAttacking = true;
                _player.SetAttack(Player.AttackStrength.Heavy);
                _player.CurrentAttackType = Player.AttackType.Heavy;

                _player.Stamina -= _player.HeavyAttackStamina;
                Player.OnStaminaChanged?.Invoke(_player.Stamina / _player.MaxStamina);
                _lastStaminaUseTime = Time.time;
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
                _player.SetAttack(Player.AttackStrength.ChargedLight);
                _player.Animator.SetBool(_player.LightAttackHash, false);
                _player.Animator.SetTrigger(_player.ChargedTriggeredHash);
            }
            else if (_player.CurrentAttackType == Player.AttackType.Heavy)
            {
                _player.SetAttack(Player.AttackStrength.ChargedHeavy);
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
                _player.SetAttack(Player.AttackStrength.ComboLight);
                _player.Animator.SetBool(_player.LightAttackHash, false);
                _player.Animator.SetTrigger(_player.Combo1AttackHash);
            }
            else if (_player.CurrentAttackType == Player.AttackType.Heavy)
            {
                _player.SetAttack(Player.AttackStrength.ComboHeavy);
                _player.Animator.SetBool(_player.HeavyAttackHash, false);
                _player.Animator.SetTrigger(_player.Combo2AttackHash);
            }
        }
        
        private void CheckForSwordCollisions()
        {
            _player.SwordCollider.transform.localPosition = _isFlipped
                ? new Vector3(-1f, _player.SwordCollider.transform.localPosition.y,
                    _player.SwordCollider.transform.localPosition.z)
                : new Vector3(1f, _player.SwordCollider.transform.localPosition.y,
                    _player.SwordCollider.transform.localPosition.z);
        }
        
        #endregion

        #region MoveLogic

        private void Move()
        {
            _player.Movement = InputReader.Instance.PlayerActions.Player.Move.ReadValue<Vector2>();
            // Actualiza grounded antes de calcular si aterrizó
            var wasGrounded = _player.IsGrounded;

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

            // Detectar aterrizaje
            bool justLanded = !wasGrounded && _player.IsGrounded;
            if (justLanded && Mathf.Abs(_player.Rigidbody2D.linearVelocity.y) > 1f)
            {
                AudioManager.Instance.PlayOneShotSFX("PlayerLand");
                _player.CaidaParticula.Play();
            }

            float targetSpeedX = _player.Movement.x * _player.Speed;
            float smoothedX =
                Mathf.SmoothDamp(_player.Rigidbody2D.linearVelocity.x, targetSpeedX, ref _currentVelocity.x, 0.1f);
            float verticalSpeed = _player.Rigidbody2D.linearVelocity.y;

            if (_player.IsGrounded)
            {
                _player.Rigidbody2D.linearVelocity = new Vector2(smoothedX, verticalSpeed);
                _player.Animator.SetFloat(_player.SpeedXHash, Mathf.Abs(_player.Movement.x));
                                
                if (Mathf.Abs(_player.Rigidbody2D.linearVelocity.x) > _player.OccurAfterVelocity)
                {
                    _counter += Time.deltaTime;
                    
                    if (_counter > _player.DustFormationPeriod)
                    {
                        _player.MovimientoParticula.Play();
                        _counter = 0;
                    }
                }
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
                    _player.canDash = false;
                }

                _dashCooldown -= Time.deltaTime;
                if (_dashCooldown <= 0)
                {
                    _isDashing = false;
                    _player.canDash = false;
                }
            }

            if (!_player.canDoubleJump && _player.IsGrounded && _player.abilities.hasDoubleJump)
            {
                _player.canDoubleJump = true;
            }

            if (!_player.canDash && _player.IsGrounded && _player.abilities.hasDash)
            {
                _player.canDash = true;
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
            {
                _player.Rigidbody2D.AddForce(Vector2.up * _player.JumpForce, ForceMode2D.Impulse);
                AudioManager.Instance.PlayRandomSFX("PlayerJump");
            }
            
            if (!context.performed || _player.IsGrounded || !_player.canDoubleJump)
                return;

            _player.canDoubleJump = false;
            // Reset vertical velocity before double jump
            _player.Rigidbody2D.linearVelocity = new Vector2(_player.Rigidbody2D.linearVelocity.x, 0f);
            _player.Rigidbody2D.AddForce(Vector2.up * (_player.JumpForce * 1.1f), ForceMode2D.Impulse);
            AudioManager.Instance.PlayRandomSFX("PlayerJump");
        }

        private void Dash(InputAction.CallbackContext context)
        {
            if (!context.performed || !_player.canDash)
                return;

            _isDashing = true;
            _dashCooldown = 0.45f;
        }

        #endregion
    }
}