using _Memoriam.Script.Enemies.BT;
using _Memoriam.Script.General;
using _Memoriam.Script.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Enemies.BasicEnemy
{
    public class BasicEnemy : BaseEnemy
    {
        private Parallel _behaviourTree;
        [SerializeField] private ParticleSystem bloodParticleEffect;

        protected override void Awake()
        {
            base.Awake();
            SetUpBehaviorTree();
        }

         private void SetUpBehaviorTree()
        {
            _behaviourTree = new Parallel("BasicEnemy_MainParallelBT");

            var detectionNode = new Leaf("DetectPlayerContinuously", new Stretegies.ActionStrategy(base.Detect), 3);
            _behaviourTree.AddChild(detectionNode);

            var mainBehaviorSelector = new PrioritySelector("MainBehaviorSelector");

            var chaseAndAttackSequence = new Sequence("ChaseAndAttackSequence");
            chaseAndAttackSequence.AddChild(new Leaf("Condition_IsPlayerDetected", new Stretegies.Condition(() => base.IsPlayerDetected), 2));
            chaseAndAttackSequence.AddChild(new Leaf("Action_MoveTowardsPlayer", new Stretegies.ActionStrategy(base.MoveTowards), 2));
            chaseAndAttackSequence.AddChild(new Leaf("Action_AttackPlayer", new Stretegies.ActionStrategy(base.Attack), 2));
            mainBehaviorSelector.AddChild(chaseAndAttackSequence);

            var patrolSequence = new Sequence("PatrolSequence");
            patrolSequence.AddChild(new Leaf("Condition_PlayerNotDetected", new Stretegies.Condition(() => !base.IsPlayerDetected), 1));
            patrolSequence.AddChild(new Leaf("Action_Patrol", new Stretegies.ActionStrategy(base.Patrol), 1));
            mainBehaviorSelector.AddChild(patrolSequence);
            
            _behaviourTree.AddChild(mainBehaviorSelector);
        }
         
        private void Update()
        {
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
            {
                if (Rb) 
                    Rb.linearVelocity = Vector2.zero;
                
                EnemyAnimator?.SetHorizontalMovement(0f);
                return;
            }

            if (this.enabled && _behaviourTree != null && Stats.CurrentHealth > 0)
            {
                _behaviourTree.Process();
            }
        }

        public override void ReceiveDamage(float damage, Vector2 position)
        {
            base.ReceiveDamage(damage, position);

            if (bloodParticleEffect != null)
            {
                Instantiate(bloodParticleEffect, transform.position, Quaternion.identity);
            }
        }

        //Call this in an animation event
        public void FinalizeDeath()
        {
            if (Stats != null && CurrentTarget != null && CurrentTarget is Player.Player realPlayer)
            {
                realPlayer.Progression.GainXp(Stats.Experience);
            }
            
            
            ObjectPool.Instance.ReturnToPool("Enemies", this.gameObject);
        }
    }
}
