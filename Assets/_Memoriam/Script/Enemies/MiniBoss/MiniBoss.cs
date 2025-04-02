using System.Collections.Generic;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Enemies.MiniBoss
{
    public class MiniBoss : BaseEnemy
    {
        [SerializeField] private float phaseTransitionHealthThreshold = 0.5f;
        [SerializeField] private float buffedDamageMultiplier = 1.5f;
        [SerializeField] private float buffedSpeedMultiplier = 1.3f;

        private bool _isBuffed;
        private bool _isTransitioning;
        private float _originalDamage;
        private float _originalSpeed;
        [SerializeField] private float globalAttackCooldown = 1.5f;
        private Dictionary<int, float> _attackWeights = new Dictionary<int, float>();
        private Dictionary<int, float> _tempWeights = new Dictionary<int, float>();
        private const float DefaultWeight = 1.0f;
        private const float WeightPenalty = 0.2f;
        private const float WeightRecoveryRate = 0.1f;

        // Attack pattern variables
        [SerializeField] private float attack1Cooldown = 3f;
        [SerializeField] private float attack2Cooldown = 4f;
        [SerializeField] private float attack3Cooldown = 5f;
        private float _lastAttack1Time;
        private float _lastAttack2Time;
        private float _lastAttack3Time;
        private int _currentAttackIndex;

        [SerializeField] private float attack1Damage = 20f;
        [SerializeField] private float attack2Damage = 15f;
        [SerializeField] private float attack3Damage = 25f;

        [SerializeField] private float attack1Range = 2f;
        [SerializeField] private float attack2Range = 3f;
        [SerializeField] private float attack3Range = 2.5f;

        private Parallel _behaviourTree;

        private readonly int _attack1Hash = Animator.StringToHash("Attack1");
        private readonly int _attack2Hash = Animator.StringToHash("Attack2");
        private readonly int _attack3Hash = Animator.StringToHash("Attack3");
        private readonly int _buffHash = Animator.StringToHash("PhaseTransition");
        
        private void Awake()
        {
            Health = MaxHealth;
            _tempWeights = new Dictionary<int, float>();
            
            // Initialize attack weights
            _attackWeights[0] = DefaultWeight; // Attack 1
            _attackWeights[1] = DefaultWeight; // Attack 2
            _attackWeights[2] = DefaultWeight; // Attack 3

            LastAttackTime = -AttackTimeOut;
            InitialAttackTimer = -AttackTimeOut;
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

            // Detection Branch
            var detectionLeaf = new Leaf("DetectPlayer", new Stretegies.ActionStrategy(Detect), 3);
            _behaviourTree.AddChild(detectionLeaf);

            // Main Behavior Branch
            var behaviorSelector = new PrioritySelector("BehaviorSelector");

            // Combat Sequence
            var combatSequence = new Sequence("CombatSequence");
            combatSequence.AddChild(new Leaf("CheckIfDetected", new Stretegies.Condition(() => EnemyDetected), 2));
            combatSequence.AddChild(new Leaf("HandlePhaseTransition",
                new Stretegies.ActionStrategy(HandlePhaseTransition), 2));
            combatSequence.AddChild(new Leaf("MoveTowardsPlayer", new Stretegies.ActionStrategy(this.MoveTowards), 2));
            combatSequence.AddChild(new Leaf("PerformAttack", new Stretegies.ActionStrategy(PerformRandomAttack), 2));

            behaviorSelector.AddChild(combatSequence);
            _behaviourTree.AddChild(behaviorSelector);
        }

        private Node.Status HandlePhaseTransition()
        {
            if (_isTransitioning) return Node.Status.Running;

            if (!_isBuffed && Health <= MaxHealth * phaseTransitionHealthThreshold)
            {
                StartPhaseTransition();
                return Node.Status.Success;
            }

            return Node.Status.Success;
        }

        private void StartPhaseTransition()
        {
            _isTransitioning = true;
            _isBuffed = true;
            _originalDamage = Damage;
            _originalSpeed = Speed;

            // Apply buffs
            Damage *= buffedDamageMultiplier;
            Speed *= buffedSpeedMultiplier;

            // TODO: Add visual effects for phase transition
            Animator.SetTrigger(_buffHash);

            _isTransitioning = false;
        }

        public override Node.Status MoveTowards()
        {
            if (_player == null)
                return Node.Status.Failure;

            var distance = Vector2.Distance(transform.position, _playerPos);

            if (distance < AttackDistance)
            {
                IsInAttackRange = true;
                Animator.SetFloat(MoveXHash, 0f);
                return Node.Status.Success;
            }

            var diff = _playerPos.x - transform.position.x;
            
            if (Mathf.Abs(diff) > MovementThreshold)
            {
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

        private Node.Status PerformRandomAttack()
        {
            if (!IsInAttackRange) 
                return Node.Status.Failure;

            // Check global cooldown
            if (Time.time - LastAttackTime < globalAttackCooldown)
                return Node.Status.Running;

            var currentTime = Time.time;
            var canAttack1 = currentTime - _lastAttack1Time >= attack1Cooldown;
            var canAttack2 = currentTime - _lastAttack2Time >= attack2Cooldown;
            var canAttack3 = currentTime - _lastAttack3Time >= attack3Cooldown;

            if (!canAttack1 && !canAttack2 && !canAttack3)
                return Node.Status.Running;

            // Update weights
            _tempWeights.Clear();
            foreach (var kvp in _attackWeights.Keys)
            {
                _tempWeights[kvp] = Mathf.Min(DefaultWeight, 
                    _attackWeights[kvp] + WeightRecoveryRate * Time.deltaTime);
            }

            foreach (var kvp in _tempWeights)
                _attackWeights[kvp.Key] = kvp.Value;

            // Calculate weighted probabilities
            float totalWeight = 0;
            var availableAttacks = new Dictionary<int, float>();
            
            if (canAttack1) totalWeight += _attackWeights[0];
            if (canAttack2) totalWeight += _attackWeights[1];
            if (canAttack3) totalWeight += _attackWeights[2];
            
            if (canAttack1) availableAttacks[0] = _attackWeights[0];
            if (canAttack2) availableAttacks[1] = _attackWeights[1];
            if (canAttack3) availableAttacks[2] = _attackWeights[2];

            // Select attack based on weights
            float randomValue = Random.Range(0, totalWeight);
            float currentSum = 0;
            int attackIndex = 0;

            foreach (var attack in availableAttacks)
            {
                currentSum += attack.Value;
                if (randomValue <= currentSum)
                {
                    attackIndex = attack.Key;
                    break;
                }
            }

            // Apply weight penalty to selected attack
            _attackWeights[attackIndex] = WeightPenalty;
            LastAttackTime = currentTime;

            switch (attackIndex)
            {
                case 0:
                    PerformAttack1();
                    _lastAttack1Time = currentTime;
                    break;
                case 1:
                    PerformAttack2();
                    _lastAttack2Time = currentTime;
                    break;
                case 2:
                    PerformAttack3();
                    _lastAttack3Time = currentTime;
                    break;
            }

            return Node.Status.Success;
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
                ObjectPool.Instance.ReturnToPool(EnemyManager.Instance.idForBasicEnemies, this.gameObject);
            }
        }

        private void OnDisable()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnStateChanged;
        }

        private void PerformAttack1()
        {
            // Basic melee slash
            Animator.SetTrigger(_attack1Hash);
        }
        
        private void PerformAttack2()
        {
            // Basic melee slash
            Animator.SetTrigger(_attack2Hash);
        }
        
        private void PerformAttack3()
        {
            // Dash attack
            Animator.SetTrigger(_attack3Hash);
        }

        public void Attack1()
        {
            var hitbox = Physics2D.OverlapCircleAll(
                AttackPoint.transform.position,
                attack1Range,
                PlayerLayer
            );

            foreach (var hit in hitbox)
            {
                if (hit.TryGetComponent<IPlayer>(out var player))
                {
                    player.ReceiveDamage(attack1Damage * (_isBuffed ? buffedDamageMultiplier : 1f), transform.position);
                }
            }
        } 
        
        public void Attack2()
        {
            var hitbox = Physics2D.OverlapCircleAll(
                AttackPoint.transform.position,
                attack1Range,
                PlayerLayer
            );

            foreach (var hit in hitbox)
            {
                if (hit.TryGetComponent<IPlayer>(out var player))
                {
                    player.ReceiveDamage(attack2Damage * (_isBuffed ? buffedDamageMultiplier : 1f), transform.position);
                }
            }
        }  
        
        public void Attack3()
        {
            // Calculate dash direction towards player
            Vector2 dashDirection = (_playerPos - (Vector2)transform.position).normalized;

            // Perform the dash
            Rigidbody2D.AddForce(dashDirection * (Speed * 2f), ForceMode2D.Impulse);

            var hitbox = Physics2D.OverlapBoxAll(
                AttackPoint.transform.position,
                new Vector2(attack3Range, 1f),
                0f,
                PlayerLayer
            );

            foreach (var hit in hitbox)
            {
                if (hit.TryGetComponent<IPlayer>(out var player))
                {
                    player.ReceiveDamage(attack3Damage * (_isBuffed ? buffedDamageMultiplier : 1f), transform.position);
                }
            }
        }


        private void OnDrawGizmosSelected()
        {
            // Draw attack ranges for debugging
            if (AttackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(AttackPoint.transform.position, attack1Range);

                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(AttackPoint.transform.position, attack2Range);

                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(AttackPoint.transform.position, new Vector3(attack3Range, 1f, 0f));
            }
        }
    }
}
