using UnityEngine;

namespace _Memoriam.Script.Powerups
{
    public class PowerUp : MonoBehaviour, IPickable
    {
        [field: SerializeField] public TypeOfPowerUp TypeOfPowerUp { get; private set; }
        
        public void Pick()
        {
            Destroy(gameObject);
        }
    }
}