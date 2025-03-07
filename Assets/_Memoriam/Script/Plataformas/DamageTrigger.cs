using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class DamageTrigger : MonoBehaviour
    {
        [SerializeField] private float damage;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IPlayer>(out var player))
            {
                Debug.Log("Player received " + damage + " damage");
                player.ReceiveDamage(damage);
            }
        }
    }
}
