using System.Collections.Generic;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.Enemies.Components; 
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
    [RequireComponent(typeof(EnemyStats), typeof(EnemyDetection), typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyCombat), typeof(EnemyAnimator), typeof(EnemySaveLoad))]
    public class BaseEnemy : MonoBehaviour, IDamageable, IEnemy
    {
        public EnemyStats Stats { get; private set; }
        public EnemyDetection Detection { get; private set; }
        public EnemyMovement Movement { get; private set; }
        public EnemyCombat Combat { get; private set; }
        public EnemyAnimator EnemyAnimator { get; private set; }
        public EnemySaveLoad SaveLoad { get; private set; }
        
        public Rigidbody2D Rb { get; private set; }
        public Animator Anim { get; private set; }
        public SpriteRenderer Sr { get; private set; }

        public IPlayer CurrentTarget { get; set; }
        public Vector2 CurrentTargetPosition { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public bool IsPlayerDetected { get; set; }
        public bool IsInAttackRangeState { get; set; } 
        [field: SerializeField] public List<Vector2> PatrolPoints { get; set; } = new List<Vector2>();

        public bool IsPerformingSpecialMovement { get; set; } = false;

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Anim = GetComponent<Animator>();
            Sr = GetComponent<SpriteRenderer>();

            Stats = GetComponent<EnemyStats>();
            Detection = GetComponent<EnemyDetection>();
            Movement = GetComponent<EnemyMovement>();
            Combat = GetComponent<EnemyCombat>();
            EnemyAnimator = GetComponent<EnemyAnimator>();
            SaveLoad = GetComponent<EnemySaveLoad>();

            Stats.Initialize(this);
            Detection.Initialize(this);
            Movement.Initialize(this);
            Combat.Initialize(this);
            EnemyAnimator.Initialize(this);
            
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        protected virtual void OnEnable()
        {
            if (Rb != null)
                Rb.bodyType = RigidbodyType2D.Dynamic;
            
            IsPlayerDetected = false;
            IsInAttackRangeState = false;
            CurrentTarget = null;
            Movement.ResetPatrol();
        }
        
        protected virtual void OnDisable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        
        protected void OnGameStateChanged(GameStateManager.GameState state)
        {
            if (state == GameStateManager.GameState.OnPause)
                EnemyAnimator.SetHorizontalMovement(0f); //
        }

        #region IEnemy Implementation (Delegación a Componentes)

        public virtual Node.Status Detect() => Detection.DetectPlayer();
        public virtual Node.Status MoveTowards() => Movement.MoveTowardsTarget();
        public virtual Node.Status Patrol() => Movement.Patrol();
        public virtual Node.Status Attack() => Combat.AttemptAttack();

        #endregion

        #region IDamageable Implementation
        
        public virtual void ReceiveDamage(float damage)
        {
            Combat.ReceiveDamage(damage);
        }
        public virtual void ReceiveDamage(float damage, Vector2 damageSourcePosition)
        {
            Combat.ReceiveDamage(damage, damageSourcePosition);
        }
        #endregion
        
        #if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Detection?.DrawGizmos();
            Combat?.DrawGizmos();
            Movement?.DrawGizmos(PatrolPoints);
        }
        #endif
    }
}