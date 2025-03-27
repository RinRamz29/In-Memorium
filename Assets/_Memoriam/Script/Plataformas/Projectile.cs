using System;
using _Memoriam.Script.General;
using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float damage;
        [SerializeField] private string iDForPool;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private Rigidbody2D rb;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.transform.TryGetComponent<IPlayer>(out var player))
            {
                player.ReceiveDamage(damage, this.transform.position);
                ObjectPool.Instance.ReturnToPool(iDForPool, gameObject);
            }
            else
            {
                ObjectPool.Instance.ReturnToPool(iDForPool, this.gameObject);
            }
        }

        private void Update()
        {
            rb.linearVelocity += Vector2.up * (projectileSpeed * Time.deltaTime);
        }
    }
}