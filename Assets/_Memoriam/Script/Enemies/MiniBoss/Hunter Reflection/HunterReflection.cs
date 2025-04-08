using UnityEngine;
using _Memoriam.Script.Enemies.Bosses;
using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.Player;

namespace _Memoriam.Script.Enemies.MiniBoss.Hunter_Reflection
{
    public class HunterReflection : BossEnemy
    {
        private float _nextMimicCheck;
        private float _mimicInterval = 2f;

        protected override Node.Status PerformAttack()
        {
            if (!IsInAttackRange) return Node.Status.Failure;

            if (Time.time >= _nextMimicCheck)
            {
                MimicPlayerAttack();
                _nextMimicCheck = Time.time + _mimicInterval;
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }

        private void MimicPlayerAttack()
        {
            if (_player == null) 
                return;

            // Simula la copia de un tipo de ataque
            int attackIndex = Random.Range(0, attackData.attacks.Length);
            ExecuteAttack(attackIndex);

            Debug.Log("El Reflejo imitó un ataque del jugador.");
        }

        protected override void OnPhaseTransition()
        {
            // Más agresivo, puede usar combos
            _mimicInterval *= 0.7f;
        }
    }
}