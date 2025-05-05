using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Plataformas;
using UnityEngine;

namespace _Memoriam.Script.Enemies.BasicEnemy
{
    public class RangedEnemy : BaseEnemy
    {
        [SerializeField] private string projectilePoolId;
        [SerializeField] private float optimalDistance = 4f;
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float retreatDistance = 2f;
        
        
        public delegate void MonsterDefeated(int exp);
        public static event MonsterDefeated OnMonsterDefeated;
        
        private Parallel _behaviourTree;
        
        private void Awake()
        {
            Health = MaxHealth;
            LastAttackTime = -AttackTimeOut;
            InitialAttackTimer = -AttackTimeOut;
            Experience = Experience;
            SetUpBehaviorSelector();
        }

        private void Start()
        {
            PatrolPoints.Add(transform.position);
            foreach (var offset in OffsetPoints)
            {
                var offsetX = (transform.position.x + offset.x);
                var offsetY = (transform.position.y + offset.y);
                PatrolPoints.Add(new Vector2(offsetX, offsetY));
            }
        }

        private void OnEnable()
        {
            GameStateManager.Instance.OnGameStateChanged += OnStateChanged;
        }

        private void SetUpBehaviorSelector()
        {
            _behaviourTree = new Parallel("BaseEnemySelector");
            
            // Continuous Detection Branch
            var detectionLeaf = new Leaf("ContinuousDetection", new Stretegies.ActionStrategy(Detect), 3);
            _behaviourTree.AddChild(detectionLeaf);
            
            // Main Behavior Branch
            var behaviorSelector = new PrioritySelector("BehaviorSelector");
            
            // Chase Logic
            var chaseSequence = new Sequence("ChaseSequence");
            chaseSequence.AddChild(new Leaf("CheckIfDetected", new Stretegies.Condition(() => EnemyDetected), 2));
            chaseSequence.AddChild(new Leaf("MoveTowardsEnemy", new Stretegies.ActionStrategy(MoveTowards), 2));
            chaseSequence.AddChild(new Leaf("Attack", new Stretegies.ActionStrategy(Attack), 2));  
            behaviorSelector.AddChild(chaseSequence);

            // Regular Patrol Logic
            var patrolSequence = new Sequence("PatrolSelector");
            patrolSequence.AddChild(new Leaf("CheckNotDetected", new Stretegies.Condition(() => !EnemyDetected), 1));
            patrolSequence.AddChild(new Leaf("Patrol", new Stretegies.ActionStrategy(Patrol), 1));
            behaviorSelector.AddChild(patrolSequence);
            
            _behaviourTree.AddChild(behaviorSelector);
        }

        public override Node.Status MoveTowards()
        {
            if (_player == null)
                return Node.Status.Failure;

            var distance = Vector2.Distance(transform.position, _playerPos);

            // If too far away, return failure
            if (distance > 8f)
            {
                EnemyDetected = false;
                return Node.Status.Failure;
            }

            // If at optimal distance, stop moving
            if (Mathf.Abs(distance - optimalDistance) < 0.5f)
            {
                Animator.SetFloat(MoveXHash, 0f);
                return Node.Status.Success;
            }

            // If too close, back away
            if (distance < retreatDistance)
            {
                var diff = _playerPos.x - transform.position.x;
                
                if (diff > 0)
                {
                    transform.position -= transform.right * (Speed * Time.deltaTime);
                    SpriteRenderer.flipX = false;
                    _isFlipped = false;
                }
                else
                {
                    transform.position += transform.right * (Speed * Time.deltaTime);
                    SpriteRenderer.flipX = true;
                    _isFlipped = true;
                }
                
                Animator.SetFloat(MoveXHash, 1f);
                return Node.Status.Running;
            }

            // If too far, move closer
            if (distance > optimalDistance)
            {
                var diff = _playerPos.x - transform.position.x;
                
                if (diff > 0)
                {
                    transform.position += transform.right * (Speed * Time.deltaTime);
                    SpriteRenderer.flipX = false;
                    _isFlipped = false;
                }
                else
                {
                    transform.position -= transform.right * (Speed * Time.deltaTime);
                    SpriteRenderer.flipX = true;
                    _isFlipped = true;
                }
                
                Animator.SetFloat(MoveXHash, 1f);
            }

            return Node.Status.Running;
        }

        private void Update()
        {
            if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
                return;
            
            _behaviourTree?.Process();
            
            if (Health <= 0)
            {
                Die();
            }
        }
        
        public override Node.Status Attack()
        {
            if (_player == null)
                return Node.Status.Failure;

            var distance = Vector2.Distance(transform.position, _playerPos);

            // Only attack if within optimal range
            if (distance < optimalDistance * 1.5f && distance > retreatDistance)
            {
                // Wait for attack cooldown
                if (Time.time - LastAttackTime > AttackTimeOut)
                {
                    Animator.SetTrigger(AttackHash);
                    
                    // Spawn projectile
                    GameObject projectileObj = ObjectPool.Instance.SpawnFromPool(projectilePoolId, firePoint.position, Quaternion.identity, true);
                    if (projectileObj.TryGetComponent<Projectile>(out var projectile))
                    {
                        projectile.Direction = (_playerPos - (Vector2)firePoint.position).normalized;
                    }
                    
                    LastAttackTime = Time.time;
                }
            }
            return Node.Status.Success;
        }

        public override void ReceiveDamage(float damage)
        {
            Animator.SetTrigger(DamagedHash);
            Health -= damage;
        }

        private void Die()
        {
            Animator.SetTrigger(DieHash);
            _behaviourTree = null;

            if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                OnMonsterDefeated(Experience);
                ObjectPool.Instance.ReturnToPool(EnemyManager.Instance.idForBasicEnemies, this.gameObject);
            }
        }

        private void OnDisable()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnStateChanged;
        }
    }
}
