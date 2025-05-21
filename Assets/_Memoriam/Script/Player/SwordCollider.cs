using System;
using System.Collections.Generic;
using _Memoriam.Script.Enemies;
using UnityEngine;

namespace _Memoriam.Script.Player
{
    public class SwordCollider : MonoBehaviour
    {
        private float _damage;
        private readonly HashSet<IEnemy> _alreadyHit = new();
        private Player _player;
        
        private void OnEnable()
        {
            _alreadyHit.Clear();
            _player = gameObject.GetComponentInParent<Player>();
        }

        public void SetData(float damage)
        {
            _damage = damage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IEnemy>(out var enemy) && !_alreadyHit.Contains(enemy))
            {
                enemy.ReceiveDamage(_damage, _player.transform.position);
                _alreadyHit.Add(enemy);
            }
        }
    }
}
