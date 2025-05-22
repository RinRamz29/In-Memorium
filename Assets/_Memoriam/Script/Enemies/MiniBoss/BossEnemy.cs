using _Memoriam.Script.Audio;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Enemies.MiniBoss
{
    public abstract class BossEnemy : BaseEnemy
    {
        [Header("Boss Phase Logic")] [SerializeField]
        protected float phaseTransitionHealthThresholdPercentage = 0.5f;

        [SerializeField] protected float buffedDamageMultiplier = 1.5f;
        [SerializeField] protected float buffedSpeedMultiplier = 1.3f;
        protected bool IsBuffed { get; set; }
        protected bool IsTransitioningPhase { get; private set; }
        protected float OriginalBossDamage { get; private set; }
        protected float OriginalBossSpeed { get; private set; }

        [Header("Boss Combat Logic")] [SerializeField]
        protected float globalAbilityCooldown = 3f;

        [SerializeField] protected MiniBossAttacks attackData;

        protected Parallel BehaviourTree;
        protected float LastBossAbilityTime = -Mathf.Infinity;
        protected readonly int BuffAnimationHash = Animator.StringToHash("PhaseTransition");

        protected override void Awake()
        {
            base.Awake();

            if (Stats != null)
            {
                OriginalBossDamage = Stats.Damage;
                OriginalBossSpeed = Stats.Speed;
            }

            SetUpBehaviorTree();
        }

        protected virtual void Update()
        {
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
            {
                if (Rb)
                    Rb.linearVelocity = Vector2.zero;
                EnemyAnimator?.SetHorizontalMovement(0f);
                return;
            }

            if (this.enabled && BehaviourTree != null && Stats != null && Stats.CurrentHealth > 0)
            {
                BehaviourTree.Process();
            }
        }

        protected virtual void SetUpBehaviorTree()
        {
            BehaviourTree = new Parallel("BossBehavior_BT_Parallel");

            var detectionNode = new Leaf("DetectPlayerContinuously", new Stretegies.ActionStrategy(base.Detect), 3);
            BehaviourTree.AddChild(detectionNode);

            var mainBehaviorSelector = new PrioritySelector("MainBehaviorSelector");

            var combatSequence = new Sequence("CombatSequence");
            combatSequence.AddChild(new Leaf("Condition_IsPlayerDetected",
                new Stretegies.Condition(() => base.IsPlayerDetected), 2)); // Usa propiedad de BaseEnemy
            combatSequence.AddChild(new Leaf("Node_HandlePhaseLogic", new Stretegies.ActionStrategy(HandlePhaseLogic),
                2));
            combatSequence.AddChild(new Leaf("Node_MoveTowardsPlayer", new Stretegies.ActionStrategy(base.MoveTowards),
                2)); // Usa BaseEnemy.MoveTowards (terrestre por defecto)
            combatSequence.AddChild(new Leaf("Node_PerformBossAttack",
                new Stretegies.ActionStrategy(PerformAttack), 2)); // Llama al método abstracto PerformAttack

            mainBehaviorSelector.AddChild(combatSequence);
            BehaviourTree.AddChild(mainBehaviorSelector);
        }

        protected virtual Node.Status HandlePhaseLogic()
        {
            if (IsTransitioningPhase)
            {
                return Node.Status.Running;
            }

            if (!IsBuffed && Stats.CurrentHealth <= Stats.MaxHealth * phaseTransitionHealthThresholdPercentage)
            {
                StartPhaseTransition();
                return IsTransitioningPhase ? Node.Status.Running : Node.Status.Success;
            }

            return Node.Status.Success;
        }

        protected virtual void StartPhaseTransition()
        {
            IsTransitioningPhase = true;

            if (!IsBuffed)
            {
                OriginalBossDamage = Stats.Damage;
                OriginalBossSpeed = Stats.Speed;
            }

            Stats.Damage = OriginalBossDamage * buffedDamageMultiplier;
            Stats.Speed = OriginalBossSpeed * buffedSpeedMultiplier;
            IsBuffed = true;

            EnemyAnimator.Anim.SetTrigger(BuffAnimationHash);

            OnPhaseTransition();

            IsTransitioningPhase = false;
        }

        protected virtual void OnPhaseTransition()
        {
        }

        protected abstract Node.Status PerformAttack();

        protected void ExecuteAttackFromData(int attackIndexInSo)
        {
            if (attackData == null || attackData.attacks == null || attackIndexInSo < 0 ||
                attackIndexInSo >= attackData.attacks.Length)
                return;

            var attack = attackData.attacks[attackIndexInSo];


            if (CurrentTarget != null && Movement != null) 
            {
                Movement.FlipTowards(CurrentTargetPosition);
            }

            if (!string.IsNullOrEmpty(attack.animationTrigger) && EnemyAnimator != null && EnemyAnimator.Anim != null)
            {
                EnemyAnimator.Anim.SetTrigger(Animator.StringToHash(attack.animationTrigger));
            }

            if (attack.attackSfx != null)
            {
                if (AudioManager.Instance != null) 
                {
                    AudioManager.Instance.PlayOneShotSFX(attack.attackSfx);
                }
                else
                {
                    var localAudio = GetComponent<AudioSource>();
                    if (localAudio == null) localAudio = gameObject.AddComponent<AudioSource>();
                    localAudio.PlayOneShot(attack.attackSfx);
                }
            }

            var effectSpawnPoint = (Combat != null && Combat.attackOriginPoint != null)
                ? Combat.attackOriginPoint.transform
                : transform;

            if (attack.effectPrefab != null)
            {
                Instantiate(attack.effectPrefab, effectSpawnPoint.position,
                    effectSpawnPoint.rotation); 
            }


            var finalDamage =
                attack.damage *
                (IsBuffed ? buffedDamageMultiplier : 1f); 

            var hitDetectionOrigin = (Combat != null && Combat.attackOriginPoint != null)
                ? Combat.attackOriginPoint.position
                : transform.position;
            var hitDetectionRotationZ = (Combat != null && Combat.attackOriginPoint != null)
                ? Combat.attackOriginPoint.eulerAngles.z
                : transform.eulerAngles.z;


            if (attack.type == MiniBossAttack.AttackType.Circle)
            {
                Collider2D[]
                    hits = Physics2D.OverlapCircleAll(hitDetectionOrigin, attack.range,
                        Stats.PlayerLayer);
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<IPlayer>(out var player))
                    {
                        player.ReceiveDamage(finalDamage,
                            transform.position); 
                    }
                }
            }
            else if (attack.type == MiniBossAttack.AttackType.Box)
            {
                Vector2 boxSize = new Vector2(attack.range, attack.width); 
                Collider2D[] hits = Physics2D.OverlapBoxAll(hitDetectionOrigin, boxSize, hitDetectionRotationZ,
                    Stats.PlayerLayer);

                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<IPlayer>(out var player))
                    {
                        player.ReceiveDamage(finalDamage, transform.position);
                    }
                }
            }
        }

        protected virtual void HandleBossDefeatSequence()
        {
            BehaviourTree = null; // Detener IA

            if (CurrentTarget != null && CurrentTarget is Player.Player realPlayer && Stats != null)
            {
                realPlayer.Progression.GainXp(Stats.Experience);
            }
        }

        public override void ReceiveDamage(float damage)
        {
            if (Stats.CurrentHealth <= 0) return;

            base.ReceiveDamage(damage);

            if (!IsBuffed && !IsTransitioningPhase &&
                Stats.CurrentHealth <= Stats.MaxHealth * phaseTransitionHealthThresholdPercentage)
            {
                StartPhaseTransition();
            }
        }

        public virtual void FinalizeBossDeath()
        {
            if (Stats != null && CurrentTarget != null && CurrentTarget is Player.Player realPlayer)
            {
                realPlayer.Progression.GainXp(Stats.Experience);
            }

            string poolIdToUse = SaveLoad?.id;
            poolIdToUse = "Miniboss";

            if (!string.IsNullOrEmpty(poolIdToUse) && ObjectPool.Instance != null)
            {
                ObjectPool.Instance.ReturnToPool(poolIdToUse, this.gameObject);
            }
        }
    }
}