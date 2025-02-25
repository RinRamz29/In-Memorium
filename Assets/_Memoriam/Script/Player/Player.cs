using System;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player.States;
using _Memoriam.Script.Player.VeilOfShadows.Hea.StateMachine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace _Memoriam.Script.Player
{
    public class Player : MonoBehaviour, IPlayer
    {
        public StateMachineBase StateMachine { get; private set; } = new();
        
        [Header("Dependencies")]
        [field: SerializeField] public Animator Animator { get; set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; set; }
        [field: SerializeField] public AudioSource AudioSource { get; set; }
        [field: SerializeField] public Rigidbody2D Rigidbody2D { get; set; }
        [field: SerializeField] public Transform GroundCheck { get; set; }
        [field: SerializeField] public float GroundDistance { get; set; } = 0.1f;
        [field: SerializeField] public LayerMask GroundMask { get; set; }
        [field: SerializeField] public LayerMask EnemyLayer { get; set; }
        [field: SerializeField] public Collider2D SwordCollider { get; set; }


        [Header("Stats")]
        [field: SerializeField] public float Health { get; set; }
        [field: SerializeField] public float MaxHealth { get; private set; }
        [field: SerializeField] public float Stamina { get; private set; }
        [field: SerializeField] public float MaxStamina { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; } = 10f;
        [field: SerializeField] public float Damage { get; set; } = 10f;
        [field: SerializeField, Range(5f, 30f)] public float Speed { get; private set; }
        
        [Inject] public PlayerActionsScript PlayerActions { get; set; }
        [Inject] public GameStateManager GameStateManager { get; set; }
        [Inject] public GameManager GameManager { get; set; }
        
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
        
        // Combo tracking
        public bool IsAttacking { get; set; }
        public bool ComboInputReceived { get; set; }
        public AttackType CurrentAttackType { get; set; }
        public bool ComboWindowOpen { get; set; }
        
        // Animation hashes
        public int LightAttackHash { get; } = Animator.StringToHash("Light");
        public int HeavyAttackHash { get; } = Animator.StringToHash("Heavy");
        public int Combo1AttackHash { get;  } = Animator.StringToHash("Combo1");
        public int Combo2AttackHash { get; } = Animator.StringToHash("Combo2");
        public int ComboTriggeredHash { get; } = Animator.StringToHash("ComboTriggered");
        public int SpeedXHash  { get; } = Animator.StringToHash("SpeedX");
        public int SpeedYHash  { get; } = Animator.StringToHash("SpeedY");
        
        // Movement parameters
        public Vector2 Movement { get; set; }
        public bool IsGrounded { get; set; }

        private void Awake()
        {
            StateMachine.ChangeState(new PlayerCombatState(this));
            Health = MaxHealth;
            Stamina = MaxStamina;
        }

        private void OnEnable()
        {
            GameStateManager.OnGameStateChanged += OnStateChanged;
        }

        private void Update()
        {
            if (GameStateManager.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;
            
            StateMachine?.Tick();
            
            if (Health <= 0)
            {
                Die();
            }
        }

        private void OnDisable()
        {
            GameStateManager.OnGameStateChanged -= OnStateChanged;
        }

        private void LateUpdate()
        {
            if (GameStateManager.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;
                
            StateMachine?.LateTick();
        }

        public float ReceiveDamage(float damage)
        {
            return Health -= damage;
        }

        private void Die()
        {
            //TO DO
            //Implement animation trigger

            GameManager.OnLose();
        }

        #region CombosLogic
        public void OpenComboWindow()
        {
            ComboWindowOpen = true;
        }

        public void CloseComboWindow()
        {
            ComboWindowOpen = false;
            
            if (!ComboInputReceived)
            {
                ResetAttackState();
            }
        }

        private void ResetAttackState()
        {
            IsAttacking = false;
            ComboInputReceived = false;
            ComboWindowOpen  = false;
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

    }
}
