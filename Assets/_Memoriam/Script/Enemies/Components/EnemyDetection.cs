using UnityEngine;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.Player;

namespace _Memoriam.Script.Enemies.Components
{
    public class EnemyDetection : MonoBehaviour
    {
        private BaseEnemy _baseEnemy;
        private EnemyStats _stats;
        private Transform _detectionOrigin; 
        private EnemyMovement _movement;

        public void Initialize(BaseEnemy baseEnemy)
        {
            _baseEnemy = baseEnemy;
            _movement = baseEnemy.Movement;
            _stats = baseEnemy.Stats;
            _detectionOrigin = _baseEnemy.transform;
        }

        public Node.Status DetectPlayer()
        {
            Collider2D[] results = Physics2D.OverlapCircleAll(_detectionOrigin.position, _stats.PlayerDetectionRadius,
                _stats.PlayerLayer);

            foreach (var coll in results)
            {
                if (coll.TryGetComponent<IPlayer>(out var player))
                {
                    if (HasLineOfSight(coll.transform))
                    {
                        _baseEnemy.CurrentTargetPosition = coll.transform.position;
                        _baseEnemy.CurrentTarget = player;
                        _baseEnemy.IsPlayerDetected = true;
                        return Node.Status.Success;
                    }
                }
            }

            _baseEnemy.IsPlayerDetected = false;
            _baseEnemy.CurrentTarget = null;
            return Node.Status.Failure;
        }

        private bool HasLineOfSight(Transform target)
        {
            Vector2 directionToTarget = (target.position - _detectionOrigin.position).normalized;
            float distanceToTarget = Vector2.Distance(_detectionOrigin.position, target.position);

            if (distanceToTarget > _stats.LineOfSightDistance) 
                return false;
            
            var hit = Physics2D.Raycast(_detectionOrigin.position, directionToTarget, distanceToTarget,
                _stats.ObstacleLayer);

            Debug.DrawRay(_detectionOrigin.position, directionToTarget * distanceToTarget,
                hit.collider == null ? Color.green : Color.red, 0.1f);

            return hit.collider == null; 
        }

        public void DrawGizmos()
        {
            if (_stats == null || _detectionOrigin == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_detectionOrigin.position, _stats.PlayerDetectionRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_detectionOrigin.position, _stats.LineOfSightDistance);
        }
    }
}
