using UnityEngine;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Plataformas;

namespace _Memoriam.Script.Enemies.BasicEnemy
{
    public class RangedEnemy : BaseEnemy
    {
        [Header("Ranged Enemy Specifics")] [SerializeField]
        private string projectilePoolId = "EnemyProjectile";

        [SerializeField] private float optimalDistance = 5f;
        [SerializeField] private float retreatDistance = 2.5f;
        [SerializeField] private float maxChaseDistanceRanged = 12f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private ParticleSystem bloodParticleEffect;

        private Parallel _behaviourTree;
        private float _lastProjectileAttackTime = -Mathf.Infinity;
        private float _lastRetreatActionTime = -Mathf.Infinity;

        protected override void Awake()
        {
            base.Awake();
            SetUpBehaviorTree();
        }

        private void SetUpBehaviorTree()
        {
            _behaviourTree = new Parallel("RangedEnemy_BT_Parallel");

            var detectionNode = new Leaf("DetectPlayerContinuously", new Stretegies.ActionStrategy(base.Detect), 3);
            _behaviourTree.AddChild(detectionNode);

            var mainBehaviorSelector = new PrioritySelector("MainBehaviorSelector");

            var engageSequence = new Sequence("EngageSequence");
            engageSequence.AddChild(new Leaf("Condition_IsPlayerDetected",
                new Stretegies.Condition(() => base.IsPlayerDetected), 2));
            engageSequence.AddChild(new Leaf("Action_MaintainDistance", new Stretegies.ActionStrategy(this.MoveTowards),
                2));
            engageSequence.AddChild(new Leaf("Action_RangedAttack", new Stretegies.ActionStrategy(this.Attack),
                2));
            mainBehaviorSelector.AddChild(engageSequence);

            var patrolSequence = new Sequence("PatrolSequence");
            patrolSequence.AddChild(new Leaf("Condition_PlayerNotDetected",
                new Stretegies.Condition(() => !base.IsPlayerDetected), 1));
            patrolSequence.AddChild(new Leaf("Action_Patrol", new Stretegies.ActionStrategy(base.Patrol),
                1));
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

        public override Node.Status MoveTowards()
        {
            if (CurrentTarget == null || !IsPlayerDetected)
            {
                Movement.StopMovement();
                return Node.Status.Failure;
            }

            Vector2 currentPosition = transform.position;
            Vector2 playerPosition = CurrentTargetPosition;
            float distanceToPlayer = Vector2.Distance(currentPosition, playerPosition);

            Movement.FlipTowards(playerPosition);

            if (distanceToPlayer > maxChaseDistanceRanged)
            {
                Movement.StopMovement();
                IsPlayerDetected = false;
                return Node.Status.Failure;
            }

            if (distanceToPlayer < retreatDistance)
            {
                Vector2 retreatDirection = (currentPosition - playerPosition).normalized;
                if (retreatDirection == Vector2.zero)
                {
                    retreatDirection = Movement.IsFlipped ? Vector2.right : Vector2.left;
                }

                if (Movement.IsGroundInDirection(retreatDirection, true))
                {
                    Movement.SetMovementIntent(retreatDirection);
                    EnemyAnimator.SetHorizontalMovement(1f);
                    return Node.Status.Running;
                }

                Movement.StopMovement();
                EnemyAnimator.SetHorizontalMovement(0f);
                return Node.Status.Success;
            }

            if (distanceToPlayer > optimalDistance + 0.5f)
            {
                var approachDirection = (playerPosition - currentPosition).normalized;
                if (Movement.IsGroundInDirection(approachDirection, false))
                {
                    Movement.SetMovementIntent(approachDirection);
                    EnemyAnimator.SetHorizontalMovement(1f);
                    return Node.Status.Running;
                }

                Movement.StopMovement();
                EnemyAnimator.SetHorizontalMovement(0f);
                return Node.Status.Success;
            }

            Movement.StopMovement();
            EnemyAnimator.SetHorizontalMovement(0f);
            return Node.Status.Success;
        }


        public override Node.Status Attack()
        {
            if (CurrentTarget == null || !IsPlayerDetected || firePoint == null)
            {
                return Node.Status.Failure;
            }

            Movement.FlipTowards(CurrentTargetPosition);

            var distanceToPlayer = Vector2.Distance(transform.position, CurrentTargetPosition);

            if (distanceToPlayer < (optimalDistance * 1.8f) && distanceToPlayer > (retreatDistance - 0.5f))
            {
                if (Time.time >= _lastProjectileAttackTime + Stats.AttackCooldown)
                {
                    EnemyAnimator.TriggerAttack();

                    SpawnProjectile();

                    _lastProjectileAttackTime = Time.time;
                    return Node.Status.Success;
                }

                return Node.Status.Running;
            }

            return Node.Status.Failure;
        }

        public void FireProjectileEvent()
        {
            SpawnProjectile();
        }

        private void SpawnProjectile()
        {
            if (CurrentTarget == null || firePoint == null) return;

            var counter = ObjectPool.Instance.GetNextCounter(projectilePoolId);
            var projectileGo =
                ObjectPool.Instance.GetReferenceFromPool(projectilePoolId, counter, firePoint.position,
                    Quaternion.identity,
                    true);

            if (projectileGo == null || !projectileGo.TryGetComponent<Projectile>(out var projectileComponent))
                return;

            var direction = (CurrentTargetPosition - (Vector2)firePoint.position).normalized;
            projectileComponent.Direction = direction;
            projectileComponent.Damage = Stats.Damage;
        }

        public override void ReceiveDamage(float damage, Vector2 damageSourcePosition)
        {
            base.ReceiveDamage(damage, damageSourcePosition);

            if (bloodParticleEffect != null)
            {
                Instantiate(bloodParticleEffect, transform.position, Quaternion.identity);
            }
        }

        public void FinalizeDeath()
        {
            if (CurrentTarget != null && CurrentTarget is Player.Player realPlayer)
            {
                realPlayer.Progression.GainXp(Stats.Experience);
            }

            if (bloodParticleEffect != null)
            {
                Instantiate(bloodParticleEffect, transform.position, Quaternion.identity);
            }

            
            ObjectPool.Instance.ReturnToPool("RangedEnemies", this.gameObject);
        }
    }
}