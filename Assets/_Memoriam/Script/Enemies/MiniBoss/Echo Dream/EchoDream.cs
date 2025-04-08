using UnityEngine;
using _Memoriam.Script.Enemies.Bosses;
using _Memoriam.Script.Enemies.BT;

namespace _Memoriam.Script.Enemies.MiniBoss.Echo_Dream
{
    public class EchoDream : BossEnemy
    {
        private int _currentPhase = 0;
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

            // Fase 1: solo ataque 0
            // Fase 2+: mezcla de ataques 1 y 2
            int[] phaseAttacks = _currentPhase == 0 ? new[] { 0 } : new[] { 1, 2 };

            foreach (int i in phaseAttacks)
            {
                var atk = attackData.attacks[i];
                if (currentTime - _lastAttackTimes[i] >= atk.cooldown)
                {
                    ExecuteAttack(i);
                    _lastAttackTimes[i] = currentTime;

                    // Simula teletransporte después de atacar
                    TeleportRandomly();
                    return Node.Status.Success;
                }
            }

            return Node.Status.Running;
        }

        protected override void OnPhaseTransition()
        {
            _currentPhase++;
            Debug.Log("Eco Onírico entra a la fase " + _currentPhase);
        }

        private void TeleportRandomly()
        {
            Vector3 offset = new Vector3(Random.Range(-3f, 3f), 0, 0);
            transform.position += offset;
        }
    }
}