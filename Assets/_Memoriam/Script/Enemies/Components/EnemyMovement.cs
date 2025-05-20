using System.Collections.Generic;
using UnityEngine;
using _Memoriam.Script.Enemies.BT;

namespace _Memoriam.Script.Enemies.Components
{
    public class EnemyMovement : MonoBehaviour
    {
        private BaseEnemy _baseEnemy;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private EnemyStats _stats;
        private EnemyAnimator _enemyAnimator;

        public bool IsFlipped { get; private set; }
        private int _currentPatrolIndex = 0;
        private float _waitTimer = 0f;
        private float _returnToPatrolTimer = 0f;
        private Vector2 _movementIntent; 
        private bool _shouldMove;
        private float _lastFlipTime;
        private const float FlipCooldown = 0.25f; 

        public void Initialize(BaseEnemy baseEnemy)
        {
            _baseEnemy = baseEnemy;
            _rb = baseEnemy.Rb;
            _sr = baseEnemy.Sr;
            _stats = baseEnemy.Stats;
            _enemyAnimator = baseEnemy.EnemyAnimator;
        }
        
        void FixedUpdate()
        {
            if (_shouldMove)
            {
                Vector2 currentVelocity = _rb.linearVelocity;
                Vector2 targetVelocity = new Vector2(_movementIntent.x * _stats.Speed, currentVelocity.y);
                _rb.linearVelocity = targetVelocity;
            }
            else
            {
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            }
            _shouldMove = false; 
        }

        public Node.Status MoveTowardsTarget()
        {
            if (_baseEnemy.IsPerformingSpecialMovement)
                return Node.Status.Running;
            
            if (_baseEnemy.CurrentTarget == null)
            {
                StopMovement();
                return Node.Status.Failure;
            }

            Vector2 targetPos = _baseEnemy.CurrentTargetPosition;
            float distanceToTarget = Vector2.Distance(transform.position, targetPos);

            FlipTowards(targetPos);

            if (distanceToTarget < _stats.AttackDistance)
            {
                StopMovement();
                _baseEnemy.IsInAttackRangeState = true;
                return Node.Status.Success; 
            }
            
            if (distanceToTarget > _stats.MaxChaseDistance)
            {
                StopMovement();
                _baseEnemy.IsPlayerDetected = false; 
                return Node.Status.Failure;
            }

            _baseEnemy.IsInAttackRangeState = false;

            if (!IsGroundAhead() && IsFalling())
            {
                 StopMovement();
                 return Node.Status.Running; 
            }
            if (!IsGroundAhead() && !IsFalling())
            {
                StopMovement();
                 return Node.Status.Failure; 
            }


            float diffX = targetPos.x - transform.position.x;
            if (Mathf.Abs(diffX) > _stats.MovementStopThreshold)
            {
                _movementIntent = new Vector2(Mathf.Sign(diffX), 0);
                _shouldMove = true;
                _enemyAnimator.SetHorizontalMovement(Mathf.Abs(_movementIntent.x));
            }
            else
            {
                StopMovement();
            }
            return Node.Status.Running;
        }

        public Node.Status Patrol()
        {
            if (_baseEnemy.IsPerformingSpecialMovement)
                return Node.Status.Running;
            
            if (_baseEnemy.PatrolPoints == null || _baseEnemy.PatrolPoints.Count == 0)
            {
                StopMovement();
                return Node.Status.Failure; 
            }

            Vector2 currentTargetPoint = _baseEnemy.PatrolPoints[_currentPatrolIndex];
            FlipTowards(currentTargetPoint);

            if (_waitTimer > 0)
            {
                _waitTimer -= Time.deltaTime;
                StopMovement();
                return Node.Status.Running; 
            }
            
            float distanceToPoint = Vector2.Distance(transform.position, currentTargetPoint);

            if (distanceToPoint < _stats.MovementStopThreshold * 2f) 
            {
                StopMovement();
                _waitTimer = _stats.WaitTimeAtPatrolPoint;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _baseEnemy.PatrolPoints.Count;
                _returnToPatrolTimer = 0f;
                return Node.Status.Running; 
            }

            if (!IsGroundAhead() && !IsFalling()) 
            {
                StopMovement();
                _waitTimer = _stats.WaitTimeAtPatrolPoint / 2f;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _baseEnemy.PatrolPoints.Count;
                return Node.Status.Running;
            }
            
            if (distanceToPoint > _stats.MaxChaseDistance / 2f) 
            {
                 _returnToPatrolTimer += Time.deltaTime;
                 if (_returnToPatrolTimer >= _stats.ReturnToPatrolTimeout)
                 {
                    transform.position = currentTargetPoint; 
                    _returnToPatrolTimer = 0f;
                    StopMovement();
                    return Node.Status.Success; 
                 }
            } else {
                 _returnToPatrolTimer = 0f; 
            }

            float diffX = currentTargetPoint.x - transform.position.x;
            _movementIntent = new Vector2(Mathf.Sign(diffX), 0);
            _shouldMove = true;
            _enemyAnimator.SetHorizontalMovement(Mathf.Abs(_movementIntent.x));

            return Node.Status.Running;
        }
        
        public void ResetPatrol()
        {
            _currentPatrolIndex = 0;
            _waitTimer = 0f;
            _returnToPatrolTimer = 0f;
        }

        public void StopMovement()
        {
            _movementIntent = Vector2.zero;
            _shouldMove = false;
            _enemyAnimator.SetHorizontalMovement(0f);
        }

        public void FlipTowards(Vector2 targetPosition)
        {
            if (Time.time < _lastFlipTime + FlipCooldown) return;

            var directionX = targetPosition.x - transform.position.x;
            if (Mathf.Abs(directionX) > _stats.FlipThreshold) 
            {
                var shouldBeFlipped = directionX < 0;
                if (IsFlipped != shouldBeFlipped)
                {
                    IsFlipped = shouldBeFlipped;
                    _sr.flipX = IsFlipped;
                    _lastFlipTime = Time.time;
                    _baseEnemy.Combat.UpdateAttackPointDirection(IsFlipped); 
                }
            }
        }
        
        public bool IsGroundAhead()
        {
            if (_stats.GroundLayer == 0) return true; 
            
            Vector2 rayOrigin = (Vector2)transform.position + 
                                (_sr.flipX ? Vector2.left : Vector2.right) * _stats.EdgeCheckHorizontalOffset + 
                                Vector2.up * _stats.EdgeCheckVerticalOffset;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, _stats.GroundCheckDistance, _stats.GroundLayer);
            return hit.collider != null;
        }

        private bool IsFalling()
        {
            return _rb.linearVelocity.y < -0.1f; 
        }

        public void ApplyKnockback(Vector3 knockBackDirection, float knockBack)
        {
            var force = knockBackDirection * knockBack;
            _baseEnemy.Rb.AddForce(force, ForceMode2D.Impulse);
        }

        public void DrawGizmos(List<Vector2> patrolPoints)
        {
            if (_stats == null) return;

            // Gizmo para IsGroundAhead
            Gizmos.color = Color.green;
            Vector2 rayOrigin = (Vector2)transform.position + 
                                (_sr != null && _sr.flipX ? Vector2.left : Vector2.right) * _stats.EdgeCheckHorizontalOffset +
                                Vector2.up * _stats.EdgeCheckVerticalOffset;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * _stats.GroundCheckDistance);

            // Gizmos para puntos de patrulla
            if (patrolPoints != null)
            {
                Gizmos.color = Color.magenta;
                for (int i = 0; i < patrolPoints.Count; i++)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i], 0.3f);
                    if (i < patrolPoints.Count -1)
                        Gizmos.DrawLine(patrolPoints[i], patrolPoints[i+1]);
                    else if (patrolPoints.Count > 1) // Conectar el último con el primero
                        Gizmos.DrawLine(patrolPoints[i], patrolPoints[0]);
                }
            }
        }
    }
}