using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class DamageTrigger : MonoBehaviour
    {
        [SerializeField] private float damage;
        [SerializeField] private COSA tipoDedanio;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IPlayer>(out var player))
            {
                Debug.Log("Player received " + damage + " damage");
                Debug.Log("Player received " + damage + " damage");

                switch (tipoDedanio)
                {
                    case COSA.PINCHOS:
                        player.ReceiveDamage(damage); // Hace daño normal
                        break;

                    case COSA.LAVA:
                        player.ReceiveDamage(9999); // Muerte instantánea
                        Debug.Log("Player died from LAVA");
                        break;
                }
            }
        }
    }
}

enum COSA
{
    PINCHOS,
    LAVA,
    PROYECTIL
}
