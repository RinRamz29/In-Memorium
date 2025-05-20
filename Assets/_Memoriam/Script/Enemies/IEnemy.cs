using _Memoriam.Script.Enemies.BT;
using UnityEngine;

namespace _Memoriam.Script.Enemies
{
    public interface IEnemy
    {
        public void ReceiveDamage(float damage);
        public void ReceiveDamage(float damage, Vector2 position);
        public Node.Status  Attack();
        
        public Node.Status  MoveTowards();
        
        public Node.Status  Patrol();

        public Node.Status  Detect();
    }
}