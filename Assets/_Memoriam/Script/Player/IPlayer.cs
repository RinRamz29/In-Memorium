namespace _Memoriam.Script.Player
{
    public interface IPlayer
    {
        public void ReceiveDamage(float damage);
        
        public void ReceiveHeal(float heal);
    }
}