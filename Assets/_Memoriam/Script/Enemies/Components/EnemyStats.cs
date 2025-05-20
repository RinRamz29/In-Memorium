using UnityEngine;

namespace _Memoriam.Script.Enemies.Components
{
    public class EnemyStats : MonoBehaviour
    {
        [Header("Core Stats")]
        [field: SerializeField] public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; }
        [field: SerializeField] public float Speed { get; set; } = 3f;
        [field: SerializeField] public float Damage { get; set; } = 10f;
        [field: SerializeField] public int Experience { get; set; } = 10;

        [Header("Detection & Range")]
        [field: SerializeField] public float PlayerDetectionRadius { get; set; } = 5f;
        [field: SerializeField] public Vector2 PlayerDetectionBoxSize { get; set; } = new Vector2(5f, 2f); // Para OverlapBox
        [field: SerializeField] public LayerMask PlayerLayer { get; set; }
        [field: SerializeField] public LayerMask ObstacleLayer { get; set; } // Para Line of Sight
        [field: SerializeField] public float LineOfSightDistance { get; set; } = 7f;


        [Header("Combat Parameters")]
        [field: SerializeField] public float AttackDistance { get; set; } = 1.5f;
        [field: SerializeField] public float AttackCooldown { get; set; } = 2f;
        [field: SerializeField] public float AttackHitboxSizeY { get; set; } = 1f; // Altura de la cápsula de ataque
        [Tooltip("Tiempo que el enemigo espera en un punto de patrulla.")]
        [field: SerializeField] public float WaitTimeAtPatrolPoint { get; set; } = 2f;

        [Header("Movement Parameters")]
        [Tooltip("Distancia mínima para considerar que el enemigo ha llegado a un punto.")]
        [field: SerializeField] public float MovementStopThreshold { get; set; } = 0.1f;
        [Tooltip("Umbral para que el jugador deba moverse antes de que el enemigo reajuste su dirección de flip.")]
        [field: SerializeField] public float FlipThreshold { get; set; } = 0.2f;
        [Tooltip("Distancia máxima que el enemigo perseguirá al jugador antes de considerar regresar.")]
        [field: SerializeField] public float MaxChaseDistance { get; set; } = 10f;
        [Tooltip("Tiempo que el enemigo intentará volver a su punto de patrulla antes de teletransportarse.")]
        [field: SerializeField] public float ReturnToPatrolTimeout { get; set; } = 5f;
        
        [Header("Ground/Edge Detection")]
        [SerializeField] public LayerMask GroundLayer;
        [SerializeField] public float GroundCheckDistance = 0.5f; 
        [SerializeField] public float EdgeCheckHorizontalOffset = 0.5f; 
        [SerializeField] public float EdgeCheckVerticalOffset = 0.1f; // Ligeramente arriba del pivote

        // Referencia a BaseEnemy si se necesita para callbacks o acceso a otros componentes no directos.
        // protected BaseEnemy baseEnemy;

        public void Initialize(BaseEnemy enemy)
        {
            // this.baseEnemy = enemy;
            CurrentHealth = MaxHealth;
        }
    }
}