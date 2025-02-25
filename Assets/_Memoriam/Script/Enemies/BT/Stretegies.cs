using System;
using UnityEngine;

namespace _Memoriam.Script.Enemies.BT
{
    public class Stretegies
    {
        public interface IStrategy
        {
            Node.Status Process();

            void Reset()
            {
            }
        }

        public class ActionStrategy : IStrategy
        {
            private readonly Func<Node.Status> _action;

            public ActionStrategy(Func<Node.Status> action)
            {
                _action = action;
            }

            public Node.Status Process()
            {
                return _action();
            }
        }

        public class Condition : IStrategy
        {
            readonly Func<bool> predicate;

            public Condition(Func<bool> predicate)
            {
                this.predicate = predicate;
            }

            public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
        }

        public class Wait : IStrategy
        {
            private readonly float _duration;
            private float _startTime;

            public Wait(float duration)
            {
                this._duration = duration;
            }

            public Node.Status Process()
            {
                if (_startTime == 0)
                {
                    _startTime = Time.time;
                }

                if (Time.time - _startTime >= _duration)
                {
                    _startTime = 0;
                    return Node.Status.Success;
                }

                return Node.Status.Running;
            }

            public void Reset()
            {
                _startTime = 0;
            }
        }
    }
}