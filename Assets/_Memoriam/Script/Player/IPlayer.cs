using UnityEngine;
namespace _Memoriam.Script.Player
{
    public interface IPlayer
    {
        public void ReceiveDamage(float damage, Vector2 damageSource);
        public void ReceiveHeal(float heal);
    }
}
