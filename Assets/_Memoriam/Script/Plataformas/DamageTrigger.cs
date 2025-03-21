using _Memoriam.Script.Player;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class DamageTrigger : MonoBehaviour
    {
        private const float Damage = 25f;
        [SerializeField] private TypeOfTrap typeOfTrap;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IPlayer>(out var player))
            {
                switch (typeOfTrap)
                {
                    case TypeOfTrap.Razors:
                        player.ReceiveDamage(Damage, (Vector2)transform.position); 
                        break;

                    case TypeOfTrap.Lava:
                        player.ReceiveDamage(9999, (Vector2)transform.position); 
                        break;
                    
                    case TypeOfTrap.ProjectileSmoke:
                    case TypeOfTrap.ProjectileRazor:
                        player.ReceiveDamage(Damage, (Vector2)transform.position);
                        break;
                }
            }
        }
    }
}

enum TypeOfTrap
{
    Lava,
    ProjectileRazor,
    ProjectileSmoke,
    Razors,
}
