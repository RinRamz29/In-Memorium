using System.Collections;
using _Memoriam.Script.Enemies.BT;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Enemies.MiniBoss.Sentinel_Of_Oblivion
{
    public class SentinelOfOblivion : BossEnemy
    {
        [Header("Sentinel Specifics")] private float[] _lastSpecificAttackTimes;
        public ParticleSystem bloodParticleEffect;

        [Header("Phase 2 Dash Ability")] [SerializeField]
        private float abilityDashSpeed = 18f;

        [SerializeField] private float abilityDashDuration = 0.25f;
        [SerializeField] private float abilityDashCooldown = 5f;
        private float _lastAbilityDashTime = -Mathf.Infinity;

        [SerializeField] private float phaseDashDuration = 0.3f;
        
        private readonly int _phaseHash = Animator.StringToHash("PhaseTransition");

        protected override void Awake()
        {
            base.Awake();

            if (attackData != null && attackData.attacks != null)
            {
                _lastSpecificAttackTimes = new float[attackData.attacks.Length];
                for (int i = 0; i < _lastSpecificAttackTimes.Length; i++)
                {
                    _lastSpecificAttackTimes[i] = -Mathf.Infinity;
                }
            }
        }

        protected override void SetUpBehaviorTree()
        {
            BehaviourTree = new Parallel("Sentinel_BT_Parallel");

            var detectionNode =
                new Leaf("DetectPlayerContinuously", new Stretegies.ActionStrategy(base.Detect),
                    10); 
            BehaviourTree.AddChild(detectionNode);

            var mainBehaviorSelector = new PrioritySelector("MainBehaviorSelector");

            var phaseLogicSequence = new Sequence("PhaseLogic_Sequence");
            phaseLogicSequence.AddChild(new Leaf("Condition_IsPlayerDetected_ForPhase",
                new Stretegies.Condition(() => base.IsPlayerDetected), 9)); 
            phaseLogicSequence.AddChild(new Leaf("Node_HandlePhaseLogic",
                new Stretegies.ActionStrategy(HandlePhaseLogic), 9)); 

            var phaseTwoRootSelector =
                new PrioritySelector("PhaseTwo_RootSelector"); 
            phaseTwoRootSelector.AddChild(new Leaf("Condition_IsPhaseTwoActive",
                new Stretegies.Condition(() => base.IsBuffed), 7)); 
            var phaseTwoActionSelector = new RandomSelector("PhaseTwo_ActionSelector");

            var phaseTwoDashSequence = new Sequence("PhaseTwo_DashSequence");
            phaseTwoDashSequence.AddChild(new Leaf("Action_PerformAbilityDash",
                new Stretegies.ActionStrategy(this.PerformAbilityDash), 6));
            phaseTwoActionSelector.AddChild(phaseTwoDashSequence);

            var phaseTwoCombatSequence = new Sequence("PhaseTwo_CombatSequence");
            phaseTwoCombatSequence.AddChild(new Leaf("Node_P2_MoveTowardsPlayer",
                new Stretegies.ActionStrategy(base.MoveTowards), 5));
            phaseTwoCombatSequence.AddChild(new Leaf("Node_P2_PerformBossAttack",
                new Stretegies.ActionStrategy(PerformAttack), 5)); 
            phaseTwoActionSelector.AddChild(phaseTwoCombatSequence);

            phaseTwoRootSelector.AddChild(phaseTwoActionSelector); 
            mainBehaviorSelector.AddChild(
                phaseTwoRootSelector); 

            var phaseOneCombatSequence = new Sequence("PhaseOne_CombatSequence");
            phaseOneCombatSequence.AddChild(new Leaf("Condition_IsPhaseOneActive",
                new Stretegies.Condition(() => !base.IsBuffed && base.IsPlayerDetected),
                4)); 
            phaseOneCombatSequence.AddChild(new Leaf("Node_P1_HandlePhaseLogic",
                new Stretegies.ActionStrategy(HandlePhaseLogic), 4)); 
            phaseOneCombatSequence.AddChild(new Leaf("Node_P1_MoveTowardsPlayer",
                new Stretegies.ActionStrategy(base.MoveTowards), 3));
            phaseOneCombatSequence.AddChild(new Leaf("Node_P1_PerformBossAttack",
                new Stretegies.ActionStrategy(PerformAttack), 3));
            mainBehaviorSelector.AddChild(phaseOneCombatSequence);

            var patrolWhenNotDetectedSequence = new Sequence("PatrolWhenNotDetected_Sequence");
            patrolWhenNotDetectedSequence.AddChild(new Leaf("Condition_PlayerNotDetected_ForPatrol",
                new Stretegies.Condition(() => !base.IsPlayerDetected), 1));
            patrolWhenNotDetectedSequence.AddChild(new Leaf("Action_PatrolBase",
                new Stretegies.ActionStrategy(base.Patrol), 1));
            mainBehaviorSelector.AddChild(patrolWhenNotDetectedSequence);

            BehaviourTree.AddChild(mainBehaviorSelector);
        }

        protected override Node.Status PerformAttack()
        {
            if (!base.IsInAttackRangeState && !IsPlayerDetected)
            {
                return Node.Status.Failure;
            }

            if (attackData == null || attackData.attacks == null || _lastSpecificAttackTimes == null ||
                _lastSpecificAttackTimes.Length != attackData.attacks.Length)
            {
                return Node.Status.Failure;
            }

            float currentTime = Time.time;

            for (int i = 0; i < attackData.attacks.Length; i++)
            {
                var specificAttack = attackData.attacks[i];
                if (currentTime >= _lastSpecificAttackTimes[i] + specificAttack.cooldown)
                {
                    ExecuteAttackFromData(i);
                    _lastSpecificAttackTimes[i] = currentTime;
                    return Node.Status.Success;
                }
            }

            return Node.Status.Running;
        }

        protected override void OnPhaseTransition()
        {
            base.OnPhaseTransition();
            Stats.Damage = OriginalBossDamage * buffedDamageMultiplier;
            Stats.Speed = OriginalBossSpeed * buffedSpeedMultiplier;
            IsBuffed = true;
            EnemyAnimator.Anim.SetTrigger(_phaseHash);

            for (int i = 0; i < _lastSpecificAttackTimes.Length; i++)
                _lastSpecificAttackTimes[i] = -Mathf.Infinity;
        }

        protected override void HandleBossDefeatSequence()
        {
            base.HandleBossDefeatSequence();

            if (bloodParticleEffect != null)
            {
                Instantiate(bloodParticleEffect, transform.position, Quaternion.identity);
            }

            // PowerUpManager.Instance.DropPowerUp(PowerUpType.Dash, transform.position);
        }

        protected virtual Node.Status PerformAbilityDash()
        {
            if (CurrentTarget == null || !IsPlayerDetected)
            {
                return Node.Status.Failure; 
            }

            if (Time.time < _lastAbilityDashTime + abilityDashCooldown)
            {
                return Node.Status.Failure; 
            }


            StartCoroutine(ExecuteAbilityDashCoroutine());
            _lastAbilityDashTime = Time.time;
            return Node.Status.Success; 
        }

        private IEnumerator ExecuteAbilityDashCoroutine()
        {
            if (Movement == null || Rb == null) yield break;

            if (CurrentTarget != null)
            {
                Movement.FlipTowards(CurrentTargetPosition);
            }

            Vector2 dashDirection = (transform.position - (Vector3)CurrentTargetPosition).normalized;

            if (dashDirection == Vector2.zero)
                dashDirection = Vector2.left;


            var originalDrag = Rb.linearDamping;
            Rb.linearDamping = 2f;
            IsPerformingSpecialMovement = true;

            Rb.linearVelocity = dashDirection * abilityDashSpeed;
            yield return new WaitForSeconds(abilityDashDuration);

            Rb.linearVelocity = Vector2.Lerp(Rb.linearVelocity, Vector2.zero, 0.5f);
            Rb.linearDamping = originalDrag;
            IsPerformingSpecialMovement = false;
        }
    }
}