using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using UnityEngine;

namespace _Memoriam.Script.Enemies.BasicEnemy
{
    public class BasicEnemy : BaseEnemy
    {
        private Parallel _behaviourTree;
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
            chaseSequence.AddChild(new Leaf("CheckIfDetected", new Stretegies.Condition(() => 
                EnemyDetected), 2));
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
                ObjectPool.Instance.ReturnToPool(EnemyManager.Instance.idForBasicEnemies, this.gameObject);
            }
        }

        private void OnDisable()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnStateChanged;
        }
    }
}
