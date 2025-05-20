using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Enemies.MiniBoss
{
    [System.Serializable]
    public class MiniBossAttack
    {
        public string name;
        public float damage;
        public float cooldown;
        public float range;
        public float width; 
        public AttackType type;
        public string animationTrigger;
        public AudioClip attackSfx;
        public GameObject effectPrefab;

        public enum AttackType
        {
            Circle,
            Box
        }
    }

    [CreateAssetMenu(menuName = "InMemoriam/BossAttackSet")]
    public class MiniBossAttacks : ScriptableObject
    {
        public MiniBossAttack[] attacks;
    }
}
