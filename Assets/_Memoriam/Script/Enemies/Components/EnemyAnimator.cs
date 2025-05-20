using UnityEngine;

namespace _Memoriam.Script.Enemies.Components
{
    public class EnemyAnimator : MonoBehaviour
    {
        public Animator Anim;
        private readonly int _moveXHash = Animator.StringToHash("MoveX");
        private readonly int _attackHash = Animator.StringToHash("Attack");
        private readonly int _dieHash = Animator.StringToHash("Die");
        private readonly int _damagedHash = Animator.StringToHash("Damaged");

        public void Initialize(BaseEnemy baseEnemy)
        {
            Anim = baseEnemy.Anim;
        }

        public void SetHorizontalMovement(float value)
        {
            Anim.SetFloat(_moveXHash, Mathf.Abs(value)); 
        }

        public void TriggerAttack()
        {
            Anim.SetTrigger(_attackHash);
        }

        public void TriggerDie()
        {
            Anim.SetTrigger(_dieHash);
        }

        public void TriggerDamaged()
        {
            Anim.SetTrigger(_damagedHash);
        }
    }
}