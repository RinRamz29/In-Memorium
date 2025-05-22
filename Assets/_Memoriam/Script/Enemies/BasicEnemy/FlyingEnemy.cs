using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Enemies.BasicEnemy
{
    public class FlyingEnemy : BaseEnemy
    {
        private Parallel _behaviourTree;

        [Header("Flying Enemy Specifics")] [SerializeField]
        private float hoverHeight = 2f;

        [SerializeField] private float floatAmplitude = 0.25f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private ParticleSystem bloodParticleEffect;

        private int _flyingPatrolIndex = 0;
        private float _flyingWaitTimer = 0f;
        private float _flyingReturnToPatrolPointTimer = 0f;
        private bool _wasChasingPlayer = false;

        protected override void Awake()
        {
            base.Awake();
            SetUpBehaviorTree();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Rb != null)
            {
                Rb.gravityScale = 0f;
                Rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            _wasChasingPlayer = false;
            _flyingReturnToPatrolPointTimer = 0f;
        }

        private void SetUpBehaviorTree()
        {
            _behaviourTree = new Parallel("FlyerEnemy_BT_Parallel");

            var detectionNode = new Leaf("DetectPlayerContinuously", new Stretegies.ActionStrategy(base.Detect), 3);
            _behaviourTree.AddChild(detectionNode);

            var mainBehaviorSelector = new PrioritySelector("MainBehaviorSelector");

            var chaseAndAttackSequence = new Sequence("ChaseAndAttackSequence");
            chaseAndAttackSequence.AddChild(new Leaf("Condition_IsPlayerDetected",
                new Stretegies.Condition(() => base.IsPlayerDetected), 2));
            chaseAndAttackSequence.AddChild(new Leaf("Action_MoveTowardsPlayer",
                new Stretegies.ActionStrategy(this.MoveTowards), 2));
            chaseAndAttackSequence.AddChild(new Leaf("Action_AttackPlayer",
                new Stretegies.ActionStrategy(base.Attack), 2));
            mainBehaviorSelector.AddChild(chaseAndAttackSequence);

            var patrolSequence = new Sequence("PatrolSequence");
            patrolSequence.AddChild(new Leaf("Condition_PlayerNotDetected",
                new Stretegies.Condition(() => !base.IsPlayerDetected), 1));
            patrolSequence.AddChild(new Leaf("Action_Patrol", new Stretegies.ActionStrategy(this.Patrol),
                1)); // Usa FlyingEnemy.Patrol
            mainBehaviorSelector.AddChild(patrolSequence);

            _behaviourTree.AddChild(mainBehaviorSelector);
        }

        private void Update()
        {
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
            {
                if (Rb)
                    Rb.linearVelocity = Vector2.zero;

                EnemyAnimator?.SetHorizontalMovement(0f);
                return;
            }

            if (this.enabled && _behaviourTree != null && Stats.CurrentHealth > 0)
            {
                _behaviourTree.Process();
            }
        }

        public override void ReceiveDamage(float damage)
        {
            base.ReceiveDamage(damage);

            if (bloodParticleEffect != null)
            {
                Instantiate(bloodParticleEffect, transform.position, Quaternion.identity);
            }
        }

        // Call this in animation event
        public void FinalizeDeath()
        {
            if (CurrentTarget != null && CurrentTarget is Player.Player realPlayer)
            {
                realPlayer.Progression.GainXp(Stats.Experience);
            }

            if (bloodParticleEffect != null)
            {
                Instantiate(bloodParticleEffect, transform.position, Quaternion.identity); // Partículas finales
            }


            ObjectPool.Instance.ReturnToPool("FlyerEnemies", this.gameObject);
        }

        public override Node.Status Patrol()
        {
            if (PatrolPoints == null || PatrolPoints.Count == 0)
            {
                EnemyAnimator.SetHorizontalMovement(0f);
                return Node.Status.Failure;
            }

            if (_wasChasingPlayer && !base.IsPlayerDetected)
            {
                _flyingPatrolIndex = 0;
                _wasChasingPlayer = false;
                base.IsInAttackRangeState = false;
            }

            var currentTargetPoint = SpawnPosition + PatrolPoints[_flyingPatrolIndex];
            var hoverTargetPosition = currentTargetPoint +
                                      new Vector2(0,
                                          hoverHeight + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude);

            if (_flyingWaitTimer > 0)
            {
                _flyingWaitTimer -= Time.deltaTime;
                EnemyAnimator.SetHorizontalMovement(0f);
                return Node.Status.Running;
            }

            Vector2 direction = (hoverTargetPosition - (Vector2)transform.position).normalized;
            float distanceToHoverTarget = Vector2.Distance(transform.position, hoverTargetPosition);

            Rb.linearVelocity = direction * Stats.Speed;

            if (Mathf.Abs(direction.x) > 0.01f)
            {
                Movement.FlipTowards(
                    hoverTargetPosition);
            }

            EnemyAnimator.SetHorizontalMovement(Mathf.Abs(direction.x) +
                                                Mathf.Abs(direction.y));

            if (distanceToHoverTarget < Stats.MovementStopThreshold * 2f)
            {
                _flyingWaitTimer = Stats.WaitTimeAtPatrolPoint;
                _flyingPatrolIndex = (_flyingPatrolIndex + 1) % PatrolPoints.Count;
                _flyingReturnToPatrolPointTimer = 0f;
                Rb.linearVelocity = Vector2.zero;
            }

            float distanceToRawPatrolPoint = Vector2.Distance(transform.position, currentTargetPoint);
            if (distanceToRawPatrolPoint > Stats.MaxChaseDistance / 1.5f)
            {
                _flyingReturnToPatrolPointTimer += Time.deltaTime;
                if (_flyingReturnToPatrolPointTimer > Stats.ReturnToPatrolTimeout)
                {
                    transform.position = currentTargetPoint;
                    Rb.linearVelocity = Vector2.zero;
                    _flyingReturnToPatrolPointTimer = 0f;
                }
            }
            else
            {
                _flyingReturnToPatrolPointTimer = 0f;
            }

            return Node.Status.Running;
        }

        public override Node.Status MoveTowards()
        {
            if (CurrentTarget == null || !IsPlayerDetected)
            {
                Rb.linearVelocity =
                    new Vector2(Rb.linearVelocity.x * 0.5f, Rb.linearVelocity.y * 0.5f);
                EnemyAnimator.SetHorizontalMovement(0f);
                _wasChasingPlayer = false;
                return Node.Status.Failure;
            }

            _wasChasingPlayer = true;
            var targetPosition =
                CurrentTargetPosition + new Vector2(0, hoverHeight);
            targetPosition.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

            var distance = Vector2.Distance(transform.position, targetPosition);

            if (distance < Stats.AttackDistance)
            {
                Rb.linearVelocity = Vector2.zero;
                EnemyAnimator.SetHorizontalMovement(0f);
                base.IsInAttackRangeState = true;
                return Node.Status.Success;
            }

            base.IsInAttackRangeState = false;

            if (distance > Stats.MaxChaseDistance)
            {
                IsPlayerDetected = false;
                CurrentTarget = null;
                Rb.linearVelocity = Vector2.zero;
                EnemyAnimator.SetHorizontalMovement(0f);
                _wasChasingPlayer = false;
                return Node.Status.Failure;
            }

            var direction = (targetPosition - (Vector2)transform.position).normalized;
            Rb.linearVelocity = direction * Stats.Speed;

            if (Mathf.Abs(direction.x) > 0.01f)
            {
                Movement.FlipTowards(CurrentTargetPosition);
            }

            EnemyAnimator.SetHorizontalMovement(Mathf.Abs(direction.x) + Mathf.Abs(direction.y));

            return Node.Status.Running;
        }
    }
}