using System;
using System.Collections;
using _Memoriam.Script.Audio;
using _Memoriam.Script.General;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player.States;
using _Memoriam.Script.Player.VeilOfShadows.Hea.StateMachine;
using _Memoriam.Script.Powerups;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Memoriam.Script.Player
{
    #region Abilities

    public enum AbilityType
    {
        Dash,
        DoubleJump,
        ShadowForm,
        Grapple
    }

    [Serializable]
    public class PlayerAbilities
    {
        public bool hasDash;
        public bool hasDoubleJump;
        public bool hasShadowForm;
        public bool hasGrapple;

        public bool HasUnlocked(AbilityType type)
        {
            return type switch
            {
                AbilityType.Dash => hasDash,
                AbilityType.DoubleJump => hasDoubleJump,
                AbilityType.ShadowForm => hasShadowForm,
                AbilityType.Grapple => hasGrapple,
                _ => false
            };
        }
    }

    #endregion

    public class Player : MonoBehaviour, IPlayer, ISaveableObject
    {
        public PlayerAbilities abilities = new PlayerAbilities();
        public StateMachineBase StateMachine { get; private set; } = new();

        [Header("Dependencies")]
        [field: SerializeField] public Animator Animator { get; set; }
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
        [field: SerializeField] public CinemachineFollow CinemachineFollow { get; private set; }
        public bool IsTouchingWall { get; set; }

        [Header("Stats")]
        [field: SerializeField] public float Health { get; set; }
        [field: SerializeField] public float MaxHealth { get; private set; }
        [field: SerializeField] public float Stamina { get; set; }
        [field: SerializeField] public float MaxStamina { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; } = 10f;
        [field: SerializeField] public float DashForce { get; private set; } = 2f;
        [field: SerializeField] public float Damage { get; set; } = 10f;
        [field: SerializeField, Range(5f, 30f)] public float Speed { get; private set; }
        [field: SerializeField] public float LighAttackStamina { get; private set; } = 25f;
        [field: SerializeField] public float HeavyAttackStamina { get; private set; } = 35f;
        [field: SerializeField] public float CameraHorizontalOffset { get; private set; } = 3f;
        [field: SerializeField] public float CameraFallYOffset { get; private set; } = -2f;
        [field: SerializeField] public float CameraJumpYOffset { get; private set; } = 1.5f;
        [field: SerializeField] public float CameraOffsetLerpSpeed { get; private set; } = 5f;

        private bool _isInvulnerable = false;
        private const float InvulnerabilityTime = 1.5f;
        [SerializeField] private float knockbackForce = 10f;
        private const float BlinkInterval = 0.1f;


        //Delegates
        private void OnStateChanged(GameStateManager.GameState state)
        {
            switch (state)
            {
                case GameStateManager.GameState.OnGameplay:
                    Rigidbody2D.gravityScale = 2f;
                    break;
                case GameStateManager.GameState.OnLose:
                    break;
                case GameStateManager.GameState.OnPause:
                    Animator.SetFloat(SpeedXHash, 0);
                    Movement = Vector2.zero;
                    Rigidbody2D.linearVelocity = Vector2.zero;
                    Rigidbody2D.gravityScale = 0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        // Animation hashes
        public int LightAttackHash { get; } = Animator.StringToHash("Light");
        public int HeavyAttackHash { get; } = Animator.StringToHash("Heavy");
        public int Combo1AttackHash { get; } = Animator.StringToHash("Combo1");
        public int Combo2AttackHash { get; } = Animator.StringToHash("Combo2");
        public int ComboTriggeredHash { get; } = Animator.StringToHash("ComboTriggered");
        public int ChargedHeavyTriggeredHash { get; } = Animator.StringToHash("ChargedHeavyTrigger");
        public int ChargedTriggeredHash { get; } = Animator.StringToHash("ChargedTriggered");
        public int SpeedXHash { get; } = Animator.StringToHash("SpeedX");
        public int SpeedYHash { get; } = Animator.StringToHash("SpeedY");

        // Movement parameters
        public Vector2 Movement { get; set; }
        public bool IsGrounded { get; set; }


        #region UnityFlow

        private void Awake()
        {
            InputReader.Instance.PlayerActions.Player.Enable();
            StateMachine.ChangeState(new PlayerCombatState(this));
            Health = MaxHealth;
            Stamina = MaxStamina;
            LastCheckPoint = transform.position;
        }

        private void OnEnable()
        {
            GameStateManager.Instance.OnGameStateChanged += OnStateChanged;
            OnHealthChanged?.Invoke(Health);
            OnStaminaChanged?.Invoke(Stamina);
        }

        private void Update()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            StateMachine?.Tick();

            if (Health <= 0)
            {
                Die();
            }
        }

        private void OnDisable()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnStateChanged;
        }

        private void FixedUpdate()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            StateMachine?.FixedTick();
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

            if (Health <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(KnockbackAndInvulnerability(damageSource));
            }
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
        public bool ChargedInputReceived { get; set; }
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
                case AttackStrength.ChargedHeavy:
                    CurrentSwingDamage = Damage * 2.5f;
                    break;
                case AttackStrength.ChargedLight:
                    CurrentSwingDamage = Damage * 1.7f;
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

            AudioManager.Instance.PlayRandomSFX("PlayerAttack");
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
        
        public void UnlockAbility(AbilityType type)
        {
            switch (type)
            {
                case AbilityType.Dash:
                    abilities.hasDash = true;
                    break;
                case AbilityType.DoubleJump:
                    abilities.hasDoubleJump = true;
                    break;
                case AbilityType.ShadowForm:
                    abilities.hasShadowForm = true;
                    break;
                case AbilityType.Grapple:
                    abilities.hasGrapple = true;
                    break;
            }

            OnPowerUpPickedUp?.Invoke((TypeOfPickable)type);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<IPickable>(out var pickUp))
            {
                switch (pickUp.TypeOfPickable)
                {
                    case TypeOfPickable.Dash:
                        UnlockAbility(AbilityType.Dash);
                        pickUp.Pick(this.gameObject);
                        break;
                    case TypeOfPickable.DoubleJump:
                        UnlockAbility(AbilityType.DoubleJump);
                        pickUp.Pick(this.gameObject);
                        break;
                    case TypeOfPickable.Grapple:
                        UnlockAbility(AbilityType.Grapple);
                        pickUp.Pick(this.gameObject);
                        break;
                    case TypeOfPickable.ShadowForm:
                        UnlockAbility(AbilityType.ShadowForm);
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
            transform.position = data.player.position;
            LastCheckPoint = data.player.position;
            abilities = data.player.abilities;
            Health = data.player.health;
            OnHealthChanged?.Invoke(Health);
        }

        public void SaveData(ref GameData data)
        {
            var player = new SavablePlayer()
            {
                position = transform.position,
                lastCheckpoint = LastCheckPoint,
                abilities = abilities,
                health = Health,
            };
            data.player = player;
        }

        #endregion
    }
}