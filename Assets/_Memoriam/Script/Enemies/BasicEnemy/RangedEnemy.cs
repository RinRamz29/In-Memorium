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
        [SerializeField] private float retreatCooldown = 2.0f;

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
                2)); // this.MoveTowards es la lógica de kiting
            engageSequence.AddChild(new Leaf("Action_RangedAttack", new Stretegies.ActionStrategy(this.Attack),
                2)); // this.Attack es el ataque con proyectil
            mainBehaviorSelector.AddChild(engageSequence);

            var patrolSequence = new Sequence("PatrolSequence");
            patrolSequence.AddChild(new Leaf("Condition_PlayerNotDetected",
                new Stretegies.Condition(() => !base.IsPlayerDetected), 1));
            patrolSequence.AddChild(new Leaf("Action_Patrol", new Stretegies.ActionStrategy(base.Patrol),
                1)); // Usa BaseEnemy.Patrol (terrestre)
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
                Movement.StopMovement(); // Usa el método del componente Movement para detenerse
                return Node.Status.Failure;
            }

            Vector2 currentPosition = transform.position;
            Vector2 playerPosition = CurrentTargetPosition; // Propiedad de BaseEnemy
            float distanceToPlayer = Vector2.Distance(currentPosition, playerPosition);

            // Voltear hacia el jugador
            Movement.FlipTowards(playerPosition); // Usa el componente Movement para voltear

            if (distanceToPlayer < retreatDistance)
            {
                if (Time.time > _lastRetreatActionTime + retreatCooldown)
                {
                    var retreatDirection = (currentPosition - playerPosition).normalized;

                    if (Movement.IsGroundAhead())
                    {
                        Movement.StopMovement();
                        EnemyAnimator.SetHorizontalMovement(1f);
                        _lastRetreatActionTime = Time.time;
                        return Node.Status.Running;
                    }

                    Movement.StopMovement();
                    EnemyAnimator.SetHorizontalMovement(0f);
                    return Node.Status.Success;
                }

                Movement.StopMovement();
                EnemyAnimator.SetHorizontalMovement(0f);
                return Node.Status.Running;
            }

            if (distanceToPlayer > optimalDistance + 0.5f)
            {
                if (Movement.IsGroundAhead())
                {
                    EnemyAnimator.SetHorizontalMovement(1f);
                    return Node.Status.Running;
                }
                
                if (!Movement.IsGroundAhead())
                {
                    Movement.StopMovement();
                    EnemyAnimator.SetHorizontalMovement(0f);
                    return Node.Status.Running;
                }
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
                ObjectPool.Instance.GetReferenceFromPool(projectilePoolId, counter, firePoint.position, Quaternion.identity,
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

            if (EnemyManager.Instance != null && !string.IsNullOrEmpty(EnemyManager.Instance.idForRangedEnemies) &&
                ObjectPool.Instance != null) 
            {
                ObjectPool.Instance.ReturnToPool(EnemyManager.Instance.idForRangedEnemies, this.gameObject);
            }
        }
    }
}