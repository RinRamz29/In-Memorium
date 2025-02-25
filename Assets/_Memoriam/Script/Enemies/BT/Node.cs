using System.Collections.Generic;
using System.Linq;

namespace _Memoriam.Script.Enemies.BT
{
    public class UntilFail : Node
    {
        public UntilFail(string name) : base(name)
        {
        }

        public override Status Process()
        {
            if (Children[0].Process() == Status.Failure)
            {
                Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }

    public class Inverter : Node
    {
        public Inverter(string name) : base(name)
        {
        }

        public override Status Process()
        {
            switch (Children[0].Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }

    public class RandomSelector : PrioritySelector
    {
        protected override List<Node> SortChildren() => Children.Shuffle().ToList();

        public RandomSelector(string name) : base(name)
        {
        }
    }

    public class PrioritySelector : Selector
    {
        private List<Node> sortedChildren;
        private List<Node> SortedChildren => sortedChildren ??= SortChildren();

        protected virtual List<Node> SortChildren() => Children.OrderByDescending(child => child.Priority).ToList();

        public PrioritySelector(string name) : base(name)
        {
        }

        public override void Reset()
        {
            base.Reset();
            sortedChildren = null;
        }

        public override Status Process()
        {
            foreach (var child in SortedChildren)
            {
                switch (child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        return Status.Success;
                    default:
                        continue;
                }
            }

            return Status.Failure;
        }
    }

    public class Selector : Node
    {
        public Selector(string name, int priority = 0) : base(name, priority)
        {
        }

        public override Status Process()
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Process())
                {
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    case Status.Running:
                        return Status.Running;
                    default:
                        CurrentChild++;
                        return Status.Running;
                }
            }

            Reset();
            return Status.Failure;
        }
    }

    public class Sequence : Node
    {
        public Sequence(string name, int priority = 0) : base(name, priority)
        {
        }

        public override Status Process()
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Process())
                {
                    case Status.Success:
                        CurrentChild++;
                        return CurrentChild == Children.Count ? Status.Success : Status.Running;
                    case Status.Failure:
                        Reset();
                        return Status.Failure;
                    case Status.Running:
                        return Status.Running;
                    default:
                        return Status.Failure;
                }
            }

            Reset();
            return Status.Success;
        }
    }

    public class Leaf : Node
    {
        private readonly Stretegies.IStrategy strategy;

        public Leaf(string name, Stretegies.IStrategy strategy, int priority) : base(name, priority)
        {
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process();

        public override void Reset() => strategy.Reset();
    }

    public class BehaviourTree : Node
    {
        public BehaviourTree(string name) : base(name)
        {
        }

        public override Status Process()
        {
            while (CurrentChild < Children.Count)
            {
                var status = Children[CurrentChild].Process();
                if (status != Status.Success)
                {
                    return status;
                }

                CurrentChild++;
            }

            return Status.Success;
        }
    }

    public class Node
    {
        public enum Status
        {
            Success,
            Failure,
            Running
        }

        public readonly string Name;
        public readonly int Priority;

        protected readonly List<Node> Children = new();
        protected int CurrentChild;

        protected Node(string name = "Node", int priority = 0)
        {
            this.Name = name;
            this.Priority = priority;
        }

        public void AddChild(Node child) => Children.Add(child);
        public virtual Status Process() => Children[CurrentChild].Process();

        public virtual void Reset()
        {
            CurrentChild = 0;
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
    }

    public class Parallel : Node
    {
        private readonly Policy successPolicy;
        private readonly Policy failurePolicy;

        public enum Policy
        {
            RequireOne,
            RequireAll
        }

        public Parallel(string name, Policy successPolicy = Policy.RequireOne,
            Policy failurePolicy = Policy.RequireOne) : base(name)
        {
            this.successPolicy = successPolicy;
            this.failurePolicy = failurePolicy;
        }

        public override Status Process()
        {
            var successCount = 0;
            var failureCount = 0;

            foreach (var child in Children)
            {
                var status = child.Process();

                if (status == Status.Success)
                    successCount++;

                if (status == Status.Failure)
                    failureCount++;
            }

            if (successPolicy == Policy.RequireAll && successCount == Children.Count)
            {
                return Status.Success;
            }

            if (failurePolicy == Policy.RequireOne && failureCount > 0)
            {
                return Status.Failure;
            }

            return Status.Running;
        }
    }
}