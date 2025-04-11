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
        [SerializeField] private float hoverHeight = 3f;
        [SerializeField] private float floatAmplitude = 0.3f;

        private int _currentPatrolIndex = 0;
        private float _waitTimer = 0f;
        
        public delegate void MonsterDefeated(int exp);
        public static event MonsterDefeated OnMonsterDefeated;

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
            GetComponent<Rigidbody2D>().gravityScale = 0f;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void SetUpBehaviorSelector()
        {
            _behaviourTree = new Parallel("FlyerEnemySelector");

            // Continuous Detection Branch
            var detectionLeaf = new Leaf("ContinuousDetection", new Stretegies.ActionStrategy(base.Detect), 3);
            _behaviourTree.AddChild(detectionLeaf);

            // Main Behavior Branch
            var behaviorSelector = new PrioritySelector("BehaviorSelector");

            // Chase Logic without attack
            var chaseSequence = new Sequence("ChaseSequence");
            chaseSequence.AddChild(new Leaf("CheckIfDetected", new Stretegies.Condition(() => EnemyDetected), 2));
            chaseSequence.AddChild(new Leaf("MoveTowardsEnemy", new Stretegies.ActionStrategy(this.MoveTowards), 2));
            chaseSequence.AddChild(new Leaf("Attack", new Stretegies.ActionStrategy(base.Attack), 2));  
            behaviorSelector.AddChild(chaseSequence);

            // Regular Patrol Logic
            var patrolSequence = new Sequence("PatrolSelector");
            patrolSequence.AddChild(new Leaf("CheckNotDetected", new Stretegies.Condition(() => !EnemyDetected), 1));
            patrolSequence.AddChild(new Leaf("Patrol", new Stretegies.ActionStrategy(this.Patrol), 1));
            behaviorSelector.AddChild(patrolSequence);

            _behaviourTree.AddChild(behaviorSelector);
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
                ObjectPool.Instance.ReturnToPool(EnemyManager.Instance.idForFlyerEnemies, this.gameObject);
            }
        }

        public override Node.Status Patrol()
        {
            if (PatrolPoints == null || PatrolPoints.Count == 0)
                return Node.Status.Failure;

            var currentPoint = PatrolPoints[_currentPatrolIndex];
            // Add hovering effect to patrol point
            var targetPos = currentPoint;
            targetPos.y += hoverHeight + Mathf.Sin(Time.time) * floatAmplitude;

            float distance = Vector2.Distance(transform.position, targetPos);

            if (_waitTimer > 0)
            {
                _waitTimer -= Time.deltaTime;
                return Node.Status.Running;
            }

            // Return to initial patrol point when losing player
            if (WasChasing && !EnemyDetected)
            {
                _currentPatrolIndex = 0;
                IsInAttackRange = false;
                WasChasing = false;
            }

            // Smooth flying movement towards patrol point
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPos,
                Speed * Time.deltaTime
            );

            // Update sprite facing and animation
            SpriteRenderer.flipX = direction.x < 0;
            Animator.SetFloat(MoveXHash, Mathf.Abs(direction.x));

            if (distance < 0.5f)
            {
                _waitTimer = WaitTimeAtPoint;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Count;
            }
            
            if (distance > 3f)
            {
                Debug.Log("Too far, returning to spawn");
                
                _returnToSpawnTimer += Time.deltaTime;
                
                SpriteRenderer.flipX = transform.position.x - currentPoint.x < 0f;
                
                if (currentPoint.x - transform.position.x > 0)
                {
                    transform.position += transform.right * (Speed * Time.deltaTime);
                }
                else
                {
                    transform.position -= transform.right * (Speed * Time.deltaTime);
                }
                
                // If taking too long to return, teleport back
                if (_returnToSpawnTimer >= ReturnToSpawnTimeout)
                {
                    transform.position = currentPoint;
                    _returnToSpawnTimer = 0f;
                }
                return Node.Status.Running;
            }

            _returnToSpawnTimer = 0f;
            return Node.Status.Running;
        }

        public override Node.Status MoveTowards()
        {
            if (_player == null)
                return Node.Status.Failure;

            var distance = Vector2.Distance(transform.position, _playerPos);

            if (distance < AttackDistance)
            {
                Debug.Log("Reached player");
                return Node.Status.Success;
            }
            var diff = _playerPos.x - transform.position.x;
            
            if (distance > 5f)
            {
                Debug.Log("Too far, returning");
                EnemyDetected = false;
                return Node.Status.Failure;
            }

            if (Mathf.Abs(diff) > MovementThreshold)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    _playerPos,
                    Speed * Time.deltaTime
                );
                
                if (diff > 0)
                {
                    SpriteRenderer.flipX = false;
                    _isFlipped = false;
                }
                else
                {
                    SpriteRenderer.flipX = true;
                    _isFlipped = true;
                }
            }

            WasChasing = true;
            return Node.Status.Running;
        }

        private void OnDisable()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnStateChanged;
        }
    }
}