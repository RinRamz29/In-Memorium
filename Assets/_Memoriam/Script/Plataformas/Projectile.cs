using System;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.General;
using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class Projectile : MonoBehaviour
    {
        [field: SerializeField] public float Damage { get; set; }
        [SerializeField] private string iDForPool;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private Rigidbody2D rb;
        public Vector2 Direction { get; set; }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.transform.TryGetComponent<IPlayer>(out var player))
            {
                player.ReceiveDamage(Damage, this.transform.position);
            }
            
            ObjectPool.Instance.ReturnToPool(iDForPool, this.gameObject);
        }

        private void Update()
        {
            // Apply velocity in the direction
            rb.linearVelocity = Direction * projectileSpeed;
        }
    }
}
