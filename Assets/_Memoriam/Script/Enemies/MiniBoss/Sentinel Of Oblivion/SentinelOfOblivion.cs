using _Memoriam.Script.Enemies.Bosses;
using _Memoriam.Script.Enemies.BT;
using UnityEngine;

namespace _Memoriam.Script.Enemies.MiniBoss.SentinelOfOblivion
{
    public class SentinelOfOblivion : BossEnemy
    {
        [Header("Sentinel Specific")]
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
            if (!IsInAttackRange)
                return Node.Status.Failure;

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
            // Activar un efecto especial como temblor o grito
            Debug.Log("Sentinel ha entrado en fase 2");
        }

        protected override void Die()
        {
            base.Die();
            Debug.Log("Sentinel derrotado, droppeando Dash...");
            // Aquí puedes hacer aparecer el PowerUp del Dash
        }
    }
}