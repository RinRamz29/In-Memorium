using System;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player;
using UnityEngine;
using Zenject;

namespace _Memoriam.Script.Enemies
{
    public class BaseEnemy : MonoBehaviour, IDamageable, IEnemy
    {
        [field: SerializeField] public float Health { get; set; }
        [field: SerializeField] public float MaxHealth { get; set; }
        [field: SerializeField] public float Speed { get; set; }
        [field: SerializeField] public float Damage { get; set; }
        [field: SerializeField] public float AttackDistance { get; set; } = 1.5f;
        [field: SerializeField] public float WaitTimeAtPoint { get; set; } = 2f;
        [field: SerializeField] public float AttackTimeOut { get; set; } = 2f;
        [field: SerializeField] public Transform[] PatrolPoints { get; set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; set; }
        [field: SerializeField] public GameObject AttackPoint { get; set; }
        
        [field: SerializeField] public float DetectRadius { get; set; }
        [field: SerializeField] public LayerMask PlayerLayer { get; set; }
        [field: SerializeField] public Animator Animator { get; set; }
        
        [Inject] protected GameManager GameManager { get; set; }
        [Inject] protected GameStateManager GameStateManager { get; set; }
        
        //Delegates
        protected void OnStateChanged(GameStateManager.GameState state)
        {
            if (state == GameStateManager.GameState.OnPause)
                Animator.SetFloat(_moveXHash, 0f);
        }
        
        protected bool EnemyDetected;
        
        private Vector2 _playerPos;
        private IPlayer _player;
        private readonly int _moveXHash = Animator.StringToHash("MoveX");
        private readonly int _attackHash = Animator.StringToHash("Attack");
        protected readonly int _dieHash = Animator.StringToHash("Die");
        protected readonly int _damagedHash = Animator.StringToHash("Damaged");
        private int _currentPatrolIndex = 0;
        private float _waitTimer = 0f;
        private float _lastAttackTime = 0f;
        

        public Node.Status Attack()
        {
            if (_player == null)
                return Node.Status.Failure;
            
            var results = Physics2D.OverlapCircleAll(AttackPoint.transform.position, AttackDistance, PlayerLayer);

            SpriteRenderer.flipX = _playerPos.x - transform.position.x < 0;
            
            foreach (var result in results)
            {
                if (result.TryGetComponent<IPlayer>(out var player) )
                {
                    if (Time.time - _lastAttackTime > AttackTimeOut)
                    {
                        Animator.SetTrigger(_attackHash);  
                        player.ReceiveDamage(Damage);
                        _lastAttackTime = Time.time;
                        return Node.Status.Success;
                    }

                    return Node.Status.Running;
                }
            }

            EnemyDetected = false;
            return Node.Status.Failure;
        }

        public Node.Status MoveTowards()
        {
            if (_player == null)
                return Node.Status.Failure;
            
            var distance = Vector2.Distance(transform.position, _playerPos);

            if (distance < AttackDistance)
            {
                Animator.SetFloat(_moveXHash, 0f);
                return Node.Status.Success;
            }
            
            SpriteRenderer.flipX = _playerPos.x - transform.position.x < 0;

            if (_playerPos.x - transform.position.x > 0)
            {
                transform.position += transform.right * (Speed * Time.deltaTime);
            }
            else
            {
                transform.position -= transform.right * (Speed * Time.deltaTime);
            }
            
            Animator.SetFloat(_moveXHash, 1f);
            return Node.Status.Running;
        }

        public Node.Status Patrol()
        {
            if (PatrolPoints == null || PatrolPoints.Length == 0)
                return Node.Status.Failure;
            
            var currentPoint = PatrolPoints[_currentPatrolIndex];
            
            var distance = Vector2.Distance(transform.position, currentPoint.position);

            if (_waitTimer > 0)
            {
                _waitTimer -= Time.deltaTime;
                return Node.Status.Running;
            }

            if (distance < 1f)
            {
                Animator.SetFloat(_moveXHash, 0f);
                _waitTimer = WaitTimeAtPoint;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Length;
                return Node.Status.Running;
            }
            
            SpriteRenderer.flipX = currentPoint.transform.position.x - transform.position.x < 0;

            if (currentPoint.transform.position.x - transform.position.x > 0)
            {
                transform.position += transform.right * (Speed * Time.deltaTime);
            }
            else
            {
                transform.position -= transform.right * (Speed * Time.deltaTime);
            }
            
            Animator.SetFloat(_moveXHash, 1f);
            return Node.Status.Running;
        }

        public Node.Status Detect()
        {
            var results = Physics2D.OverlapCircleAll(transform.position, DetectRadius, PlayerLayer);
            
            foreach (var coll in results)
            {
                if (coll.TryGetComponent<IPlayer>(out var player))
                {
                    _playerPos = coll.transform.position;
                    _player = player;
                    EnemyDetected = true;
                    return Node.Status.Success;
                }
            }
            
            EnemyDetected = false;
            return Node.Status.Failure;
        }
        
        public virtual void ReceiveDamage(float damage)
        {
        }
    }
}