using System;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.General;
using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float damage;
        [SerializeField] private string iDForPool;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float maxLifetime = 5f;
        public Vector2 Direction { get; set; }
        
        private float _spawnTime;

        private void OnEnable()
        {
            _spawnTime = Time.time;
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.transform.TryGetComponent<IPlayer>(out var player))
            {
                player.ReceiveDamage(damage, this.transform.position);
                ObjectPool.Instance.ReturnToPool(iDForPool, gameObject);
            }
            else if (!other.transform.TryGetComponent<BaseEnemy>(out var enem))
            {
                ObjectPool.Instance.ReturnToPool(iDForPool, this.gameObject);
            }
        }

        private void Update()
        {
            // Apply velocity in the direction
            rb.linearVelocity = Direction * projectileSpeed;
            
            // Return to pool if lifetime exceeded
            if (Time.time - _spawnTime > maxLifetime)
            {
                ObjectPool.Instance.ReturnToPool(iDForPool, gameObject);
            }
        }
    }
}
