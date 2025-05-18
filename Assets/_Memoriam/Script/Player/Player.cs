using System;
using System.Collections;
using System.Linq;
using _Memoriam.Script.Audio;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player.States;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using _Memoriam.Script.Tutorial;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Memoriam.Script.Player
{
    #region Abilities

    [Serializable]
    public class PlayerAbilities
    {
        public bool hasDash;
        public bool hasDoubleJump;
        public bool hasShadowForm;
        public bool hasGrapple;
    }

    #endregion

    public class Player : MonoBehaviour, IPlayer, ISaveableObject
    {
        public PlayerAbilities abilities = new PlayerAbilities();
        public StateMachineBase StateMachine { get; private set; }

        [Header("Dependencies")]
        [field: SerializeField]
        public Animator Animator { get; set; }

        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; set; }
        [field: SerializeField] public CinemachineCamera CineMachineCamera { get; set; }
        [field: SerializeField] public Rigidbody2D Rigidbody2D { get; set; }
        [field: SerializeField] public Transform GroundCheck { get; set; }
        [field: SerializeField] public float GroundDistance { get; set; } = 0.1f;
        [field: SerializeField] public LayerMask GroundMask { get; set; }
        [field: SerializeField] public LayerMask EnemyLayer { get; set; }
        [field: SerializeField] public GameObject SwordCollider { get; set; }
        [field: SerializeField] public Transform[] WallCheck { get; set; }
        [field: SerializeField] public float WallCheckDistance { get; set; } = 0.2f;
        [field: SerializeField] public float WallSlideSpeed { get; set; } = 2f;
        [field: SerializeField] public CinemachineFollow CinemachineFollow { get; set; }

        [Header("Stats")]
        [field: SerializeField] public float Health { get; set; }

        [field: SerializeField] public float MaxHealth { get; set; }
        [field: SerializeField] public float Stamina { get; set; }
        [field: SerializeField] public float MaxStamina { get; set; }
        [field: SerializeField] public float JumpForce { get; private set; } = 10f;
        [field: SerializeField] public float DashForce { get; private set; } = 2f;
        [field: SerializeField] public float Damage { get; set; } = 10f;
        [field: SerializeField] public PlayerProgression Progression { get; set; } = new PlayerProgression();

        [field: SerializeField, Range(5f, 30f)]
        public float Speed { get; private set; }

        [field: SerializeField] public float LighAttackStamina { get; private set; } = 25f;
        [field: SerializeField] public float HeavyAttackStamina { get; private set; } = 35f;
        [field: SerializeField] public float CameraHorizontalOffset { get; private set; } = 3f;
        [field: SerializeField] public float CameraFallYOffset { get; private set; } = -2f;
        [field: SerializeField] public float CameraJumpYOffset { get; private set; } = 1.5f;
        [field: SerializeField] public float CameraOffsetLerpSpeed { get; private set; } = 5f;

        [Header("Particles")]
        [field: SerializeField]
        public ParticleSystem MovimientoParticula { get; private set; }

        [field: SerializeField] public ParticleSystem ParticlesHeal { get; private set; }
        [field: SerializeField] public ParticleSystem LevelUpParticles { get; private set; }
        [field: SerializeField] public ParticleSystem CaidaParticula { get; private set; }
        [field: SerializeField] public ParticleSystem SaltoIzquierda { get; private set; }
        [field: SerializeField] public ParticleSystem SaltoDerecha { get; private set; }

        [field: SerializeField, Range(0, 10)] public int OccurAfterVelocity { get; private set; }

        [field: SerializeField, Range(0, 0.2f)]
        public float DustFormationPeriod { get; private set; }

        public static Action<bool> onPlayerFirstTp;
        public bool ReachedFirstTp { get; set; }
        private bool _isInvulnerable = false;
        private const float InvulnerabilityTime = 1.5f;
        [SerializeField] private float knockbackForce = 10f;
        private const float BlinkInterval = 0.1f;
        
        private Vector3 _savedVelocity;
        private float _savedAngularVelocity;


        // Animation hashes
        private int LightAttackHash { get; } = Animator.StringToHash("Light");
        private int HeavyAttackHash { get; } = Animator.StringToHash("Heavy");
        private int Combo1AttackHash { get; } = Animator.StringToHash("Combo1");
        private int Combo2AttackHash { get; } = Animator.StringToHash("Combo2");
        private int ComboTriggeredHash { get; } = Animator.StringToHash("ComboTriggered");
        private int SpeedXHash { get; } = Animator.StringToHash("SpeedX");
        private int SpeedYHash { get; } = Animator.StringToHash("SpeedY");

        // Movement parameters
        private Vector2 Movement { get; set; }
        private bool IsGrounded { get; set; }
        private bool IsTouchingWall { get; set; }



        #region UnityFlow

        private void Awake()
        {
            ResetPlayer();
        }

        private void OnEnable()
        {
            GameStateManager.Instance.OnGameStateChanged += OnStateChanged;
            InputReader.Instance.PlayerActions.Player.Enable();
            PlayerProgression.OnLevelUp += HandleLevelUp;
            onPlayerFirstTp += (cond) => ReachedFirstTp = cond;
            Health = MaxHealth;
            Stamina = MaxStamina;
            LastCheckPoint = transform.position;
            OnHealthChanged?.Invoke(Health);
            OnStaminaChanged?.Invoke(Stamina);
        }

        private void Update()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
            {
                return;
            }

            if (GameStateManager.Instance.GameCurrentState == GameStateManager.GameState.OnGameplay)
            {
                StateMachine?.Tick();
            }

            if (Health <= 0)
            {
                Die();
            }
        }

        private void OnDisable()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnStateChanged;
            PlayerProgression.OnLevelUp -= HandleLevelUp;
        }

        private void FixedUpdate()
        {
            if (GameStateManager.Instance.GameCurrentState == GameStateManager.GameState.OnGameplay)
            {
                StateMachine?.FixedTick();
            }
        }

        public void ResetPlayer()
        {
            StateMachine = new StateMachineBase();

            if (!TryGetComponent<Rigidbody2D>(out var rb))
                rb = gameObject.AddComponent<Rigidbody2D>();

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            Rigidbody2D = rb;

            if (SwordCollider == null || SwordCollider is null)
            {
                var sword = new GameObject("SwordSpawned");
                sword.transform.SetParent(transform);
                sword.transform.localPosition = new Vector3(0.95f, 0.032f, 0f);
                sword.AddComponent<SwordCollider>();
                var capsule = sword.AddComponent<CapsuleCollider2D>();
                capsule.isTrigger = true;
                capsule.size = new Vector2(1f, 2f);
                capsule.direction = CapsuleDirection2D.Vertical;
                sword.SetActive(false);
                SwordCollider = sword;
            }

            if (!TryGetComponent<Animator>(out var animator))
                animator = gameObject.AddComponent<Animator>();

            animator.Rebind();
            animator.Update(0);
            Animator = animator;

            if (GroundCheck == null)
            {
                var ground = new GameObject("GroundCheck");
                ground.transform.SetParent(transform);
                ground.transform.localPosition = new Vector3(0f, -0.67f, 0f);
                GroundCheck = ground.transform;
            }

            if (WallCheck == null || WallCheck.Length == 0)
            {
                var up = new GameObject("WallCheckUp");
                var med = new GameObject("WallCheckMed");
                var low = new GameObject("WallCheckLow");
                up.transform.SetParent(transform);
                med.transform.SetParent(transform);
                low.transform.SetParent(transform);
                med.transform.localPosition = new Vector3(0f, 0f, 0f);
                up.transform.localPosition = new Vector3(0f, 0.4f, 0f);
                low.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                WallCheck = new[] { up.transform, med.transform, low.transform };
            }

            var cam = FindFirstObjectByType<CinemachineCamera>();
            cam.Follow = transform;
            CineMachineCamera = cam;
            CinemachineFollow = cam?.GetComponent<CinemachineFollow>();

            EnemyLayer = 1 << LayerMask.NameToLayer("Enemy");
            GroundMask = 1 << LayerMask.NameToLayer("Ground");

            if (TutorialManager.Instance.CurrentStepIndex + 1 < TutorialManager.Instance.steps.Count)
                StateMachine.ChangeState(new PlayerTutorialState(this));
            else
                StateMachine.ChangeState(new PlayerCombatState(this));
        }

        #endregion

        #region LevelUpLogic

        private void HandleLevelUp(int newLevel)
        {
            MaxHealth += 10 + newLevel * 0.5f;
            Health = MaxHealth;

            MaxStamina += 5 + newLevel * 0.5f;
            Stamina = MaxStamina;

            Damage += 1 + newLevel * 0.2f;

            LevelUpParticles?.Play();
            OnHealthChanged?.Invoke(Health / MaxHealth);
            OnStaminaChanged?.Invoke(Stamina / MaxStamina);
        }

        #endregion

        #region HealDamageLogic

        // Health/Dying logic
        public Vector3 LastCheckPoint { get; set; }
        public static Action<float> OnHealthChanged { get; set; }
        public static Action<float> OnStaminaChanged { get; set; }
        public static Action<TypeOfPickable> OnPowerUpPickedUp { get; set; }

        public void ReceiveDamage(float damage, Vector2 damageSource)
        {
            if (_isInvulnerable || Health <= 0) return;

            Health -= damage;
            OnHealthChanged?.Invoke(Health / MaxHealth);
            AudioManager.Instance.PlayRandomSFX("PlayerDamage");

            if (Health <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(KnockbackAndInvulnerability(damageSource));
            }
        }

        public void PlayFootstep()
        {
            AudioManager.Instance.PlayRandomSFX("PlayerWalk");
        }

        private IEnumerator KnockbackAndInvulnerability(Vector2 damageSource)
        {
            _isInvulnerable = true;

            //Calculate the knockback
            Vector2 knockbackDirection = ((Vector2)transform.position - damageSource).normalized;

            //Ensure there is a notable horizontal knockback
            knockbackDirection = new Vector2(knockbackDirection.x, Mathf.Abs(knockbackDirection.y) * 0.5f).normalized;

            Rigidbody2D.linearVelocity = Vector2.zero;
            Rigidbody2D.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            //Visual blink effect
            StartCoroutine(BlinkEffect());

            //Invulnerability time
            yield return new WaitForSeconds(InvulnerabilityTime);

            _isInvulnerable = false;
        }

        private IEnumerator BlinkEffect()
        {
            while (_isInvulnerable)
            {
                SpriteRenderer.enabled = !SpriteRenderer.enabled;
                yield return new WaitForSeconds(BlinkInterval);
            }

            SpriteRenderer.enabled = true;
        }

        public void ReceiveHeal(float heal)
        {
            if (Health + heal >= MaxHealth)
                Health = MaxHealth;
            else
                Health += heal;

            ParticlesHeal?.Play();
            var clampedHealth = Health / MaxHealth;
            OnHealthChanged?.Invoke(clampedHealth);
        }

        private void Die()
        {
            //TO DO
            //Implement animation trigger
            this.transform.position = LastCheckPoint;
            Health = MaxHealth / 2;
            OnHealthChanged.Invoke(Health / MaxHealth);
            CineMachineCamera.Lens.OrthographicSize = 6f;
        }

        #endregion

        #region CombosLogic

        // Combo tracking
        public bool IsAttacking { get; set; }
        public bool ComboInputReceived { get; set; }
        public AttackType CurrentAttackType { get; set; }
        public bool ComboWindowOpen { get; set; }
        public float CurrentSwingDamage { get; private set; }

        public enum AttackStrength
        {
            Light,
            Heavy,
            ChargedHeavy,
            ChargedLight,
            ComboLight,
            ComboHeavy
        }

        public void SetAttack(AttackStrength type)
        {
            switch (type)
            {
                case AttackStrength.Light:
                    CurrentSwingDamage = Damage * 1.0f;
                    break;
                case AttackStrength.Heavy:
                    CurrentSwingDamage = Damage * 1.5f;
                    break;
                case AttackStrength.ComboLight:
                    CurrentSwingDamage = Damage * 1.5f;
                    break;
                case AttackStrength.ComboHeavy:
                    CurrentSwingDamage = Damage * 2.0f;
                    break;
            }
        }

        public void OpenComboWindow()
        {
            ComboWindowOpen = true;
        }

        public void EnableSwordCollider()
        {
            SwordCollider.SetActive(true);

            if (SwordCollider.TryGetComponent<SwordCollider>(out var logic))
            {
                logic.SetData(CurrentSwingDamage);
            }
        }

        public void DisableSwordCollider()
        {
            SwordCollider.SetActive(false);
        }

        public void CloseComboWindow()
        {
            ComboWindowOpen = false;
            Animator.SetBool(ComboTriggeredHash, false);
            ResetAttackState();
        }

        private void ResetAttackState()
        {
            IsAttacking = false;
            ComboInputReceived = false;
            ComboWindowOpen = false;
            CurrentAttackType = AttackType.None;


            Animator.SetBool(HeavyAttackHash, false);
            Animator.SetBool(LightAttackHash, false);
        }

        private void ResetComboState()
        {
            IsAttacking = false;
            CurrentAttackType = AttackType.None;

            Animator.SetBool(ComboTriggeredHash, false);
        }

        public enum AttackType
        {
            None,
            Light,
            Heavy
        }

        #endregion

        #region PowerUpsLogic

        public bool canDoubleJump;
        public bool canDash;

        public void UnlockAbility(TypeOfPickable type)
        {
            switch (type)
            {
                case TypeOfPickable.Dash:
                    abilities.hasDash = true;
                    break;
                case TypeOfPickable.DoubleJump:
                    abilities.hasDoubleJump = true;
                    break;
                case TypeOfPickable.ShadowForm:
                    abilities.hasShadowForm = true;
                    break;
                case TypeOfPickable.Grapple:
                    abilities.hasGrapple = true;
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<IPickable>(out var pickUp))
            {
                switch (pickUp.TypeOfPickable)
                {
                    case TypeOfPickable.Dash:
                        UnlockAbility(TypeOfPickable.Dash);
                        OnPowerUpPickedUp?.Invoke(TypeOfPickable.Dash);
                        pickUp.Pick(this.gameObject);
                        break;
                    case TypeOfPickable.DoubleJump:
                        UnlockAbility(TypeOfPickable.DoubleJump);
                        OnPowerUpPickedUp?.Invoke(TypeOfPickable.DoubleJump);
                        pickUp.Pick(this.gameObject);
                        break;
                    case TypeOfPickable.Grapple:
                        UnlockAbility(TypeOfPickable.Grapple);
                        OnPowerUpPickedUp?.Invoke(TypeOfPickable.Grapple);
                        pickUp.Pick(this.gameObject);
                        break;
                    case TypeOfPickable.ShadowForm:
                        UnlockAbility(TypeOfPickable.ShadowForm);
                        OnPowerUpPickedUp?.Invoke(TypeOfPickable.ShadowForm);
                        pickUp.Pick(this.gameObject);
                        break;
                    case TypeOfPickable.CheckPoint:
                    case TypeOfPickable.HealthPotion:
                        pickUp.Pick(this.gameObject);
                        break;
                }
            }
        }

        #endregion

        #region SaveLoadLogic

        public void LoadData(GameData data)
        {
            transform.position = data.player.lastCheckpoint;
            LastCheckPoint = data.player.lastCheckpoint;
            abilities = data.player.abilities;
            Health = data.player.health;
            Progression.Level = data.player.level;
            Progression.CurrentXp = data.player.xp;

            OnHealthChanged?.Invoke(Health / MaxHealth);
            OnStaminaChanged?.Invoke(Stamina / MaxStamina);
        }

        public void SaveData(ref GameData data)
        {
            var player = new SavablePlayer()
            {
                lastCheckpoint = LastCheckPoint,
                abilities = abilities,
                health = Health,
                level = Progression.Level,
                xp = Progression.CurrentXp,
            };
            data.player = player;
        }

        #endregion

        #region AttackLogic

        public bool IsFlipped { get; set; }
        public Vector2 Direction { get; set; }

        public bool IsDashing { get; set; }
        public float DashCooldown { get; set; }
        public float AirControlMultiplier { get; set; } = 0.75f;

        public float LastStaminaUseTime { get; set; } = -Mathf.Infinity;
        [field: SerializeField] public float StaminaRegenDelay { get; set; } = 1f;
        [field: SerializeField] public float StaminaRegenRate { get; set; } = 20f;

        private float _counter;
        private Vector2 _currentVelocity = Vector2.zero;
        private Vector3 _targetOffset;

        public void LightAttack(InputAction.CallbackContext context)
        {
            if (Animator == null)
                return;

            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            if (!context.performed)
                return;

            if (Stamina < LighAttackStamina)
            {
                return;
            }

            if (!IsAttacking)
            {
                IsAttacking = true;
                SetAttack(Player.AttackStrength.Light);
                CurrentAttackType = Player.AttackType.Light;

                Stamina -= LighAttackStamina;
                Player.OnStaminaChanged?.Invoke(Stamina / MaxStamina);
                LastStaminaUseTime = Time.time;
                CheckForSwordCollisions();
                Animator.SetBool(LightAttackHash, true);
                AudioManager.Instance.PlayRandomSFX("PlayerLightAttack");
            }
        }

        public void HeavyAttack(InputAction.CallbackContext context)
        {
            if (Animator == null)
                return;

            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            if (!context.performed)
                return;

            if (Stamina < HeavyAttackStamina)
            {
                return;
            }

            if (!IsAttacking)
            {
                IsAttacking = true;
                SetAttack(Player.AttackStrength.Heavy);
                CurrentAttackType = Player.AttackType.Heavy;

                Stamina -= HeavyAttackStamina;
                Player.OnStaminaChanged?.Invoke(Stamina / MaxStamina);
                LastStaminaUseTime = Time.time;
                CheckForSwordCollisions();
                Animator.SetBool(HeavyAttackHash, true);
                AudioManager.Instance.PlayRandomSFX("PlayerHeavyAttack");
            }
        }

        public void ExecuteCombo()
        {
            if (Animator == null)
                return;

            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            ComboInputReceived = false;
            ComboWindowOpen = false;

            CheckForSwordCollisions();
            Animator.SetBool(ComboTriggeredHash, true);
            AudioManager.Instance.PlayRandomSFX("PlayerLightAttack");
            if (CurrentAttackType == Player.AttackType.Light)
            {
                SetAttack(Player.AttackStrength.ComboLight);
                Animator.SetBool(LightAttackHash, false);
                Animator.SetTrigger(Combo1AttackHash);
            }
            else if (CurrentAttackType == Player.AttackType.Heavy)
            {
                SetAttack(Player.AttackStrength.ComboHeavy);
                Animator.SetBool(HeavyAttackHash, false);
                Animator.SetTrigger(Combo2AttackHash);
            }
        }

        private void CheckForSwordCollisions()
        {
            if (SwordCollider is null || SwordCollider == null)
                return;

            SwordCollider.transform.localPosition = IsFlipped
                ? new Vector3(-1f, SwordCollider.transform.localPosition.y,
                    SwordCollider.transform.localPosition.z)
                : new Vector3(1f, SwordCollider.transform.localPosition.y,
                    SwordCollider.transform.localPosition.z);
        }

        #endregion

        #region MoveLogic

        public void Move()
        {
            Movement = InputReader.Instance.PlayerActions.Player.Move.ReadValue<Vector2>();
            // Actualiza grounded antes de calcular si aterrizó
            var wasGrounded = IsGrounded;

            IsGrounded =
                Physics2D.OverlapCircle(GroundCheck.position, GroundDistance, GroundMask);
            IsTouchingWall = false;

            foreach (var check in WallCheck)
            {
                var direction = IsFlipped ? Vector2.left : Vector2.right;
                var hit = Physics2D.Raycast(check.position, direction, WallCheckDistance, GroundMask);
                Debug.DrawRay(check.position, direction * WallCheckDistance, Color.red);

                if (hit.collider != null)
                {
                    IsTouchingWall = true;

                    if (!IsGrounded || Rigidbody2D.linearVelocity.y > 0)
                        IsGrounded = false;

                    break;
                }
            }

            // Detectar aterrizaje
            bool justLanded = !wasGrounded && IsGrounded;
            if (justLanded && Mathf.Abs(Rigidbody2D.linearVelocity.y) > 1f)
            {
                AudioManager.Instance.PlayOneShotSFX("PlayerLand");
                CaidaParticula.Play();
            }

            float targetSpeedX = Movement.x * Speed;
            float smoothedX =
                Mathf.SmoothDamp(Rigidbody2D.linearVelocity.x, targetSpeedX, ref _currentVelocity.x, 0.1f);
            float verticalSpeed = Rigidbody2D.linearVelocity.y;

            if (IsGrounded)
            {
                Rigidbody2D.linearVelocity = new Vector2(smoothedX, verticalSpeed);
                Animator.SetFloat(SpeedXHash, Mathf.Abs(Movement.x));

                if (Mathf.Abs(Rigidbody2D.linearVelocity.x) > OccurAfterVelocity)
                {
                    _counter += Time.deltaTime;

                    if (_counter > DustFormationPeriod)
                    {
                        MovimientoParticula.Play();
                        _counter = 0;
                    }
                }
            }
            else if (IsTouchingWall && !IsGrounded)
            {
                verticalSpeed = Mathf.Max(Rigidbody2D.linearVelocity.y, -WallSlideSpeed);
                Rigidbody2D.linearVelocity = new Vector2(0f, verticalSpeed);
            }
            else
            {
                float airSpeedX = Mathf.SmoothDamp(Rigidbody2D.linearVelocity.x,
                    Movement.x * AirControlMultiplier * Speed, ref _currentVelocity.x, 0.15f);
                Rigidbody2D.linearVelocity = new Vector2(airSpeedX, verticalSpeed);
                Animator.SetFloat(SpeedXHash, 0);
            }

            if (IsDashing)
            {
                Rigidbody2D.linearVelocity =
                    new Vector2(Rigidbody2D.linearVelocity.x, Rigidbody2D.linearVelocity.y);
                Direction = IsFlipped ? Vector2.left : Vector2.right;
                Rigidbody2D.AddForce(Direction * DashForce, ForceMode2D.Impulse);

                if ((Direction.x < -0.1f && Movement.x > 0.1f) ||
                    (Direction.x > 0.1f && Movement.x < -0.1f))
                {
                    IsDashing = false;
                    canDash = false;
                }

                DashCooldown -= Time.deltaTime;
                if (DashCooldown <= 0)
                {
                    IsDashing = false;
                    canDash = false;
                }
            }

            if (!canDoubleJump && IsGrounded && abilities.hasDoubleJump)
            {
                canDoubleJump = true;
            }

            if (!canDash && IsGrounded && abilities.hasDash)
            {
                canDash = true;
            }

            UpdateCameraOffset();

            var moveX = Movement.normalized.x;

            // Horizontal offset based on direction
            if (moveX > 0.1f)
            {
                SpriteRenderer.flipX = false;
                IsFlipped = false;
            }
            else if (moveX < -0.1f)
            {
                SpriteRenderer.flipX = true;
                IsFlipped = true;
            }

            float yVelocity = Rigidbody2D.linearVelocity.y;
            if (yVelocity > 0.1f)
                Animator.SetFloat(SpeedYHash, 1);
            else if (yVelocity < -0.1f)
                Animator.SetFloat(SpeedYHash, -1);
            else
                Animator.SetFloat(SpeedYHash, 0);
        }

        public void UpdateCameraOffset()
        {
            var moveX = Movement.x;

            switch (moveX)
            {
                // Horizontal offset based on direction
                case > 0.9f:
                    _targetOffset.x = CameraHorizontalOffset;
                    break;
                case < -0.8f:
                    _targetOffset.x = -CameraHorizontalOffset;
                    break;
            }

            _targetOffset.y = CinemachineFollow.FollowOffset.y;
            _targetOffset.z = CinemachineFollow.FollowOffset.z;

            // Lerp for smooth transition
            Vector3 currentOffset = CinemachineFollow.FollowOffset;
            CinemachineFollow.FollowOffset = Vector3.Lerp(currentOffset, _targetOffset,
                Time.deltaTime * CameraOffsetLerpSpeed);
        }


        public void Jump(InputAction.CallbackContext context)
        {
            if (Rigidbody2D == null || Animator == null)
                return;

            if (!context.performed ||
                GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            if (context.performed && IsGrounded)
            {
                Rigidbody2D.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
                AudioManager.Instance.PlayRandomSFX("PlayerJump");
            }

            if (!context.performed || IsGrounded || !canDoubleJump)
                return;

            canDoubleJump = false;
            // Reset vertical velocity before double jump
            Rigidbody2D.linearVelocity = new Vector2(Rigidbody2D.linearVelocity.x, 0f);
            Rigidbody2D.AddForce(Vector2.up * (JumpForce * 1.1f), ForceMode2D.Impulse);
            AudioManager.Instance.PlayRandomSFX("PlayerJump");
            //SaltoDerecha.Play();
            //SaltoIzquierda.Play();
        }

        public void Dash(InputAction.CallbackContext context)
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            if (!context.performed || !canDash)
                return;

            IsDashing = true;
            DashCooldown = 0.45f;
            AudioManager.Instance.PlayOneShotSFX("PlayerDash");
        }

        #endregion

        #region TutoLogic

        public void OnTutorialLoaded(TutorialStep step)
        {
            Subscribe(step);
            TutorialManager.OnTutorialLoaded -= OnTutorialLoaded;
        }

        public void Subscribe(TutorialStep step)
        {
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

        public void OnStepCompleted(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            if (TutorialManager.Instance.CurrentStepIndex + 1 >= TutorialManager.Instance.steps.Count)
            {
                TutorialManager.Instance.EndTutorial();
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


        public void PowerUpPickedUp(TypeOfPickable powerUpType)
        {
            if (powerUpType != TypeOfPickable.DoubleJump && powerUpType != TypeOfPickable.Dash)
                return;

            TutorialManager.Instance.SetCanvas(true);
            TutorialManager.Instance.TutoActive = true;
        }

        public void ReachedFirstTeleport(bool condition)
        {
            if (TutorialManager.Instance.steps[TutorialManager.Instance.CurrentStepIndex].action ==
                TutorialStep.ActionType.Interact && condition)
            {
                TutorialManager.Instance.SetCanvas(true);
                TutorialManager.Instance.TutoActive = true;
            }
        }

        public void OnDoubleJump(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !canDoubleJump)
                return;

            TutorialManager.Instance.SetCanvas(false);
            TutorialManager.Instance.TutoActive = false;
            AdvanceIfCorrect();
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !canDash)
                return;

            TutorialManager.Instance.SetCanvas(false);
            TutorialManager.Instance.TutoActive = false;
            AdvanceIfCorrect();
        }

        public void AdvanceIfCorrect()
        {
            TutorialManager.Instance.NextStep();
            Subscribe(TutorialManager.Instance.steps[TutorialManager.Instance.CurrentStepIndex]);
        }

        public void Unsubscribe()
        {
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

        #region PhysicsLogic

        private void PausePhysics()
        {
            Animator.SetFloat(SpeedXHash, 0);
            _savedVelocity = Rigidbody2D.linearVelocity;
            _savedAngularVelocity = Rigidbody2D.angularVelocity;
            Rigidbody2D.angularVelocity = 0f;
            Rigidbody2D.linearVelocity = Vector2.zero;
            Rigidbody2D.gravityScale = 0f;
        }

        private void ResumePhysics()
        {
            Rigidbody2D.gravityScale = 2f;
            Rigidbody2D.linearVelocity = _savedVelocity;
            Rigidbody2D.angularVelocity = _savedAngularVelocity;
        }

        #endregion

        #region Delegates

        private void OnStateChanged(GameStateManager.GameState state)
        {
            switch (state)
            {
                case GameStateManager.GameState.OnGameplay:
                    ResumePhysics();
                    break;
                case GameStateManager.GameState.OnPause:
                    PausePhysics();
                    break;
            }
        }

        #endregion
    }
}