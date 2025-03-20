using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class DamageTrigger : MonoBehaviour
    {
        private const float Damage = 25f;
        [SerializeField] private TypeOfTrap tipoDedanio;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IPlayer>(out var player))
            {
                switch (tipoDedanio)
                {
                    case TypeOfTrap.Razors:
                        player.ReceiveDamage(Damage, (Vector2)transform.position); // PASAMOS LA POSICI�N DEL DA�O
                        break;

                    case TypeOfTrap.Lava:
                        player.ReceiveDamage(9999, (Vector2)transform.position); // Muerte instant�nea
                        break;
                }
            }
        }
    }
}

enum TypeOfTrap
{
    Lava,
    Projectile,
    Razors,
}
