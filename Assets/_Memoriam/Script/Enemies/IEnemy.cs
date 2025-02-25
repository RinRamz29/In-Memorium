using _Memoriam.Script.Enemies.BT;

namespace _Memoriam.Script.Enemies
{
    public interface IEnemy
    {
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public void ReceiveDamage(float damage);
        public Node.Status  Attack();
        
        public Node.Status  MoveTowards();
        
        public Node.Status  Patrol();

        public Node.Status  Detect();
    }
}