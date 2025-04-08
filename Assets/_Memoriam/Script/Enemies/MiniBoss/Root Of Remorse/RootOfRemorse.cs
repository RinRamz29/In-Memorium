using UnityEngine;
using _Memoriam.Script.Enemies.Bosses;
using _Memoriam.Script.Enemies.BT;

namespace _Memoriam.Script.Enemies.MiniBoss.Root_Of_Remorse
{
    public class RootOfRemorse : BossEnemy
    {
        private float[] _lastAttackTimes;

        protected override void Awake()
        {
            base.Awake();
            _lastAttackTimes = new float[attackData.attacks.Length];
            for (int i = 0; i < _lastAttackTimes.Length; i++)
                _lastAttackTimes[i] = -999f;
        }

        protected override Node.Status PerformAttack()
        {
            if (!IsInAttackRange) return Node.Status.Failure;

            float currentTime = Time.time;

            for (int i = 0; i < attackData.attacks.Length; i++)
            {
                var atk = attackData.attacks[i];
                if (currentTime - _lastAttackTimes[i] >= atk.cooldown)
                {
                    ExecuteAttack(i);
                    _lastAttackTimes[i] = currentTime;
                    return Node.Status.Success;
                }
            }

            return Node.Status.Running;
        }

        protected override void OnPhaseTransition()
        {
            // Ejemplo: más raíces emergen, o aparecen trampas en el suelo
            Debug.Log("La raíz se enfurece...");
        }
    }
}