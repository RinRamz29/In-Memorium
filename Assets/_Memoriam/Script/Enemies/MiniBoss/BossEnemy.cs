using System.Collections.Generic;
using UnityEngine;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.Enemies.MiniBoss;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player;

namespace _Memoriam.Script.Enemies.Bosses
{
    public abstract class BossEnemy : BaseEnemy
    {
        [Header("Phase Logic")]
        [SerializeField] protected float phaseTransitionHealthThreshold = 0.5f;
        [SerializeField] protected float buffedDamageMultiplier = 1.5f;
        [SerializeField] protected float buffedSpeedMultiplier = 1.3f;
        protected bool IsBuffed;
        protected bool IsTransitioning;
        protected float OriginalDamage;
        protected float OriginalSpeed;

        [Header("Combat Logic")]
        [SerializeField] protected float globalAttackCooldown = 1.5f;
        protected new float LastAttackTime;

        [SerializeField] protected MiniBossAttacks attackData;
        protected Parallel BehaviourTree;

        protected readonly int BuffHash = Animator.StringToHash("PhaseTransition");

        protected virtual void Awake()
        {
            Health = MaxHealth;
            LastAttackTime = -AttackTimeOut;
            InitialAttackTimer = -AttackTimeOut;

            SetUpBehaviorTree();
        }

        protected virtual void OnEnable()
        {
            GameStateManager.Instance.OnGameStateChanged += OnStateChanged;
        }

        protected virtual void OnDisable()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnStateChanged;
        }

        protected virtual void Update()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;

            BehaviourTree?.Process();

            if (Health <= 0)
            {
                Die();
            }
        }

        protected virtual void SetUpBehaviorTree()
        {
            BehaviourTree = new Parallel("BossBehavior");

            var detectionLeaf = new Leaf("DetectPlayer", new Stretegies.ActionStrategy(Detect), 3);
            BehaviourTree.AddChild(detectionLeaf);

            var behaviorSelector = new PrioritySelector("BehaviorSelector");

            var combatSequence = new Sequence("CombatSequence");
            combatSequence.AddChild(new Leaf("CheckDetected", new Stretegies.Condition(() => EnemyDetected), 2));
            combatSequence.AddChild(new Leaf("PhaseLogic", new Stretegies.ActionStrategy(HandlePhaseLogic), 2));
            combatSequence.AddChild(new Leaf("MoveTowardsPlayer", new Stretegies.ActionStrategy(MoveTowards), 2));
            combatSequence.AddChild(new Leaf("Attack", new Stretegies.ActionStrategy(PerformAttack), 2));

            behaviorSelector.AddChild(combatSequence);
            BehaviourTree.AddChild(behaviorSelector);
        }

        protected virtual Node.Status HandlePhaseLogic()
        {
            if (IsTransitioning)
                return Node.Status.Running;

            if (!IsBuffed && Health <= MaxHealth * phaseTransitionHealthThreshold)
            {
                StartPhaseTransition();
                return Node.Status.Success;
            }

            return Node.Status.Success;
        }

        protected virtual void StartPhaseTransition()
        {
            IsTransitioning = true;
            IsBuffed = true;

            OriginalDamage = Damage;
            OriginalSpeed = Speed;

            Damage *= buffedDamageMultiplier;
            Speed *= buffedSpeedMultiplier;

            Animator.SetTrigger(BuffHash);

            OnPhaseTransition(); // hook para hijos
            IsTransitioning = false;
        }

        protected virtual void OnPhaseTransition() { }

        protected abstract Node.Status PerformAttack();

        protected void ExecuteAttack(int index)
        {
            if (attackData == null || attackData.attacks.Length <= index)
                return;

            var attack = attackData.attacks[index];

            // Animación
            Animator.SetTrigger(Animator.StringToHash(attack.animationTrigger));

            // Sonido
            if (attack.attackSfx != null)
                AudioSource.PlayClipAtPoint(attack.attackSfx, transform.position);

            // Efecto visual
            if (attack.effectPrefab != null)
                Instantiate(attack.effectPrefab, AttackPoint.transform.position, Quaternion.identity);

            // Hit detection
            if (attack.type == MiniBossAttack.AttackType.Circle)
            {
                var hits = Physics2D.OverlapCircleAll(AttackPoint.transform.position, attack.range, PlayerLayer);
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<IPlayer>(out var player))
                        player.ReceiveDamage(attack.damage * (IsBuffed ? buffedDamageMultiplier : 1f), transform.position);
                }
            }
            else if (attack.type == MiniBossAttack.AttackType.Box)
            {
                var size = new Vector2(attack.range, attack.width);
                var hits = Physics2D.OverlapBoxAll(AttackPoint.transform.position, size, 0f, PlayerLayer);
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<IPlayer>(out var player))
                        player.ReceiveDamage(attack.damage * (IsBuffed ? buffedDamageMultiplier : 1f), transform.position);
                }
            }
        }
        
        protected virtual void Die()
        {
            Animator.SetTrigger(DieHash);
            BehaviourTree = null;

            if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                ObjectPool.Instance.ReturnToPool(EnemyManager.Instance.idForMiniBoss, gameObject);
            }
        }

        public override void ReceiveDamage(float damage)
        {
            Animator.SetTrigger(DamagedHash);
            Health -= damage;
        }
    }
}
