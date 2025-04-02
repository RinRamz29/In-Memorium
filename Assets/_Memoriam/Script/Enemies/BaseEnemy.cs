using System.Collections.Generic;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using _Memoriam.Script.Player;
using _Memoriam.Script.SaveLoad;
using _Memoriam.Script.SaveLoad.Data;
using UnityEngine;

namespace _Memoriam.Script.Enemies
{
    public class BaseEnemy : MonoBehaviour, IDamageable, IEnemy, ISaveableObject
    {
        [field: SerializeField] public string id { get; set; }
        [field: SerializeField] public float Health { get; set; }
        [field: SerializeField] public float MaxHealth { get; set; }
        [field: SerializeField] public float Speed { get; set; }
        [field: SerializeField] public float Damage { get; set; }
        [field: SerializeField] public float AttackDistance { get; set; } = 1.5f;
        [field: SerializeField] public float WaitTimeAtPoint { get; set; } = 2f;
        [field: SerializeField] public float AttackTimeOut { get; set; } = 2f;
        [field: SerializeField] public float MovementThreshold { get; set; } = 0.1f;
        [field: SerializeField] public float ReturnToSpawnTimeout { get; set; } = 5f;
        protected float _returnToSpawnTimer = 0f;
        protected bool WasChasing;
        protected float InitialAttackTimer;
        protected bool IsInAttackRange;
        protected bool TooFarAway;

        [field: SerializeField] public List<Vector2> OffsetPoints { get; set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; set; }
        [field: SerializeField] public Rigidbody2D Rigidbody2D { get; set; }
        [field: SerializeField] public GameObject AttackPoint { get; set; }
        [field: SerializeField] public GameObject DetectPoint { get; set; }

        [field: SerializeField] public Vector2 DetectRadius { get; set; }
        [field: SerializeField] public LayerMask PlayerLayer { get; set; }
        [field: SerializeField] public Animator Animator { get; set; }

        //Delegates
        protected void OnStateChanged(GameStateManager.GameState state)
        {
            if (state == GameStateManager.GameState.OnPause)
                Animator.SetFloat(MoveXHash, 0f);
        }

        protected bool EnemyDetected;
        [field: SerializeField] protected List<Vector2> PatrolPoints { get; set; } = new List<Vector2>();
        protected Vector2 _playerPos;
        protected IPlayer _player;
        protected readonly int MoveXHash = Animator.StringToHash("MoveX");
        protected readonly int AttackHash = Animator.StringToHash("Attack");
        protected readonly int DieHash = Animator.StringToHash("Die");
        protected readonly int DamagedHash = Animator.StringToHash("Damaged");
        private int _currentPatrolIndex = 0;
        private float _waitTimer = 0f;
        protected float LastAttackTime = 0f;
        protected bool _isFlipped;


        public virtual Node.Status Attack()
        {
            if (_player == null)
                return Node.Status.Failure;

            // Check if enemy is in attack range
            var sizeOfCapsule = _isFlipped ? new Vector2(-AttackDistance, 1f) : new Vector2(AttackDistance, 1f);
            AttackPoint.transform.localPosition = _isFlipped
                ? new Vector3(-1f, AttackPoint.transform.localPosition.y, AttackPoint.transform.localPosition.z)
                : new Vector3(1f, AttackPoint.transform.localPosition.y, AttackPoint.transform.localPosition.z);

            var results = Physics2D.OverlapCapsuleAll(AttackPoint.transform.position, sizeOfCapsule,
                CapsuleDirection2D.Horizontal, PlayerLayer);

            foreach (var result in results)
            {
                if (result.TryGetComponent<IPlayer>(out var player))
                {
                    // Initialize attack timer when first entering attack range
                    if (!IsInAttackRange)
                    {
                        IsInAttackRange = true;
                        InitialAttackTimer = Time.time;
                        return Node.Status.Running;
                    }

                    // Wait for both initial delay and attack timeout
                    if (Time.time - InitialAttackTimer > AttackTimeOut &&
                        Time.time - LastAttackTime > AttackTimeOut)
                    {
                        Animator.SetTrigger(AttackHash);
                        player.ReceiveDamage(Damage, transform.position);
                        LastAttackTime = Time.time;
                        return Node.Status.Success;
                    }

                    return Node.Status.Running;
                }
            }

            EnemyDetected = false;
            IsInAttackRange = false;
            return Node.Status.Failure;
        }

        public virtual Node.Status MoveTowards()
        {
            if (_player == null)
                return Node.Status.Failure;

            var distance = Vector2.Distance(transform.position, _playerPos);

            if (distance < AttackDistance)
            {
                Animator.SetFloat(MoveXHash, 0f);
                return Node.Status.Success;
            }

            if (distance > 5f)
            {
                Debug.Log("Too far, returning");
                EnemyDetected = false;
                return Node.Status.Failure;
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

            WasChasing = true;
            return Node.Status.Running;
        }

        public virtual Node.Status Patrol()
        {
            if (PatrolPoints == null || PatrolPoints.Count == 0)
                return Node.Status.Failure;
            

            var currentPoint = PatrolPoints[_currentPatrolIndex];

            var distance = Vector2.Distance(transform.position, currentPoint);

            if (_waitTimer > 0)
            {
                _waitTimer -= Time.deltaTime;
                return Node.Status.Running;
            }

            SpriteRenderer.flipX = currentPoint.x - transform.position.x < 0;

            if (currentPoint.x - transform.position.x > 0)
            {
                transform.position += transform.right * (Speed * Time.deltaTime);
            }
            else
            {
                transform.position -= transform.right * (Speed * Time.deltaTime);
            }

            if (distance < 1f)
            {
                Animator.SetFloat(MoveXHash, 0f);
                _waitTimer = WaitTimeAtPoint;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Count;
                return Node.Status.Running;
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
            Animator.SetFloat(MoveXHash, 1f);
            return Node.Status.Running;
        }

        public Node.Status Detect()
        {
            var results = Physics2D.OverlapCapsuleAll(DetectPoint.transform.position, DetectRadius,
                CapsuleDirection2D.Horizontal, PlayerLayer);

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

        public void LoadData(GameData data)
        {
            foreach (var kvp in data.EnemySavable)
            {
                if (kvp.Key == id)
                {
                    gameObject.SetActive(kvp.Value.isAlive);
                    transform.position = kvp.Value.position;
                }
            }
        }

        public void SaveData(ref GameData data)
        {
            var instance = new SavableEnemy()
            {
                isAlive = gameObject.activeInHierarchy,
                position = this.transform.position,
            };

            if (data.EnemySavable.ContainsKey(id))
            {
                data.EnemySavable.Remove(id);
            }

            data.EnemySavable.Add(id, instance);
        }
    }
}