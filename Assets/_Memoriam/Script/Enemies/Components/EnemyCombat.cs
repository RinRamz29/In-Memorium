using UnityEngine;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.Player;
using _Memoriam.Script.General; // Para IDamageable si es necesario

namespace _Memoriam.Script.Enemies.Components
{
    public class EnemyCombat : MonoBehaviour
    {
        private BaseEnemy _baseEnemy;
        private EnemyStats _stats;
        private EnemyAnimator _enemyAnimator;
        private SpriteRenderer _sr;

        private float _lastAttackTime = -Mathf.Infinity; 
        
        [Tooltip("Point where the attack OverlapCapsule initiates. Must be a child of the enemy prefab.")]
        [SerializeField] public Transform attackOriginPoint; 

        public void Initialize(BaseEnemy baseEnemy)
        {
            _baseEnemy = baseEnemy;
            _stats = baseEnemy.Stats;
            _enemyAnimator = baseEnemy.EnemyAnimator;
            _sr = baseEnemy.Sr;

            if (attackOriginPoint == null)
            {
                var foundAP = transform.Find("AttackPoint");
                if (foundAP != null) attackOriginPoint = foundAP;
                else 
                {
                    GameObject apGo = new GameObject("AttackPoint");
                    apGo.transform.SetParent(transform);
                    apGo.transform.localPosition = new Vector3(_stats.AttackDistance / 2f, 0, 0); 
                    attackOriginPoint = apGo.transform;
                }
                Debug.LogWarning($"AttackOriginPoint not assigned for: {gameObject.name}. Finding/Creation was tried", this);
            }
        }

        public Node.Status AttemptAttack()
        {
            if (_baseEnemy.CurrentTarget == null || !_baseEnemy.IsPlayerDetected)
            {
                _baseEnemy.IsInAttackRangeState = false;
                return Node.Status.Failure;
            }

            if (Time.time < _lastAttackTime + _stats.AttackCooldown)
            {
                return Node.Status.Running; 
            }

            // X is AttackDistance, Y is HitboxSize
            // HorizontalDirection
            var capsuleSize = new Vector2(_stats.AttackDistance, _stats.AttackHitboxSizeY);
            var hitPlayers = Physics2D.OverlapCapsuleAll(attackOriginPoint.position, capsuleSize, CapsuleDirection2D.Horizontal, 0f, _stats.PlayerLayer);

            foreach (var hitPlayerColl in hitPlayers)
            {
                if (hitPlayerColl.TryGetComponent<IPlayer>(out var player))
                {
                    _baseEnemy.IsInAttackRangeState = true; 
                    
                    _enemyAnimator.TriggerAttack();
                    _lastAttackTime = Time.time;
                    
                    // Do not apply damage here if an animation event is used
                    // player.ReceiveDamage(_stats.Damage, transform.position); 
                    
                    // Succes return to indicate Attack was initiated
                    return Node.Status.Success; 
                }
            }
            
            // If no player was found
            _baseEnemy.IsInAttackRangeState = false;
            return Node.Status.Failure;
        }

        // Function for animation event to apply damage
        public void ApplyAttackDamage()
        {
            if (_baseEnemy.CurrentTarget == null || !_baseEnemy.IsPlayerDetected) return;

            //Reevaluate if player is still within range of Attack at the moment this method
            // triggers to apply damage
            var capsuleSize = new Vector2(_stats.AttackDistance, _stats.AttackHitboxSizeY);
            var hitPlayers = Physics2D.OverlapCapsuleAll(attackOriginPoint.position, capsuleSize, CapsuleDirection2D.Horizontal, 0f, _stats.PlayerLayer);

            foreach (var hitPlayerColl in hitPlayers)
            {
                if (hitPlayerColl.TryGetComponent<IPlayer>(out var player))
                {
                    player.ReceiveDamage(_stats.Damage, transform.position);
                    return; 
                }
            }
        }

        public void UpdateAttackPointDirection(bool isFlipped)
        {
            attackOriginPoint.localPosition = new Vector3(
                isFlipped ? -Mathf.Abs(attackOriginPoint.localPosition.x) : Mathf.Abs(attackOriginPoint.localPosition.x),
                attackOriginPoint.localPosition.y,
                attackOriginPoint.localPosition.z
            );
        }

        public void ReceiveDamage(float damage)
        {
            _stats.CurrentHealth -= damage;
            _enemyAnimator.TriggerDamaged();

            if (_stats.CurrentHealth <= 0)
            {
                Die();
            }
        }
        
        public void ReceiveDamage(float damage, Vector2 damageSourcePosition)
        {
            ReceiveDamage(damage); 
            _baseEnemy.Movement.ApplyKnockback((transform.position - (Vector3)damageSourcePosition).normalized, 2f);
        }

        private void Die()
        {
            _enemyAnimator.TriggerDie();
            GetComponent<Collider2D>().enabled = false;
            _baseEnemy.enabled = false; 
            this.enabled = false; 
            _baseEnemy.Movement.enabled = false; 
        }
        
        public void DrawGizmos()
        {
            if (_stats == null || attackOriginPoint == null) return;

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(attackOriginPoint.position, attackOriginPoint.rotation, attackOriginPoint.lossyScale);
            DrawWireCapsule(Vector3.zero, Quaternion.identity, _stats.AttackHitboxSizeY / 2f, _stats.AttackDistance / 2f, CapsuleDirection2D.Horizontal);
            Gizmos.matrix = Matrix4x4.identity;
        }

        // Helper to draw capsule attack range since Unity does not have one for Capsule
        private static void DrawWireCapsule(Vector3 center, Quaternion rotation, float radius, float height, CapsuleDirection2D direction)
        {
            // Very simplfied
            if (direction == CapsuleDirection2D.Horizontal)
            {
                 Gizmos.DrawWireSphere(center + Vector3.left * (height - radius), radius);
                 Gizmos.DrawWireSphere(center + Vector3.right * (height - radius), radius);
                 Gizmos.DrawLine(center + Vector3.left * (height-radius) + Vector3.up * radius, center + Vector3.right * (height-radius) + Vector3.up * radius);
                 Gizmos.DrawLine(center + Vector3.left * (height-radius) + Vector3.down * radius, center + Vector3.right * (height-radius) + Vector3.down * radius);
            }
            else 
            {
                Gizmos.DrawWireSphere(center + Vector3.up * (height - radius), radius);
                Gizmos.DrawWireSphere(center + Vector3.down * (height - radius), radius);
                Gizmos.DrawLine(center + Vector3.up * (height-radius) + Vector3.left * radius, center + Vector3.down * (height-radius) + Vector3.left * radius);
                Gizmos.DrawLine(center + Vector3.up * (height-radius) + Vector3.right * radius, center + Vector3.down * (height-radius) + Vector3.right * radius);
            }
        }
    }
}