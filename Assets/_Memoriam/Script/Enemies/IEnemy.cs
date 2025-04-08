using _Memoriam.Script.Enemies.BT;

namespace _Memoriam.Script.Enemies
{
    public interface IEnemy
    {
        public void ReceiveDamage(float damage);
        public Node.Status  Attack();
        
        public Node.Status  MoveTowards();
        
        public Node.Status  Patrol();

        public Node.Status  Detect();
    }
}