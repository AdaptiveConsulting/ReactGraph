using System;

namespace ReactGraph.NodeInfo
{
    class ReadOnlyNodeInfo<T> : IValueSource<T>
    {
        // TODO Why do we can cache the current value for expressions?
        readonly Maybe<T> currentValue = new Maybe<T>();
        // would be good to not have this depdendency on the repoisitory here
        readonly NodeRepository nodeRepository;
        readonly string pathFromParent;
        readonly Func<T, T> getValue;
        bool shouldTrackChanges;

        public ReadOnlyNodeInfo(Func<T, T> getValue, string fullPath, string pathFromParent, NodeRepository nodeRepository, bool shouldTrackChanges)
        {
            FullPath = fullPath;
            this.getValue = getValue;
            this.pathFromParent = pathFromParent;
            this.nodeRepository = nodeRepository;
            this.shouldTrackChanges = shouldTrackChanges;

            // TODO Jake, why do we evaluate? This evaluates formulas at construction time (and execute actions, ie. .Do) ??
            ValueChanged();
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

        public void TrackChanges()
        {
            // I don't understand what is the purpose of this. WHat does it mean to "track" an expression?
            shouldTrackChanges = true;
        }

        public void SetTarget(ITakeValue<T> targetNode)
        {
            // TODO I wrote this, for current value support in formulas. I think this is wrong: if the target property is changed manually, the cached value (currentValue) would become stale. 
            // we probably need to read from the property all the time

            // we know that the target is a ReadWriteNode (ie. a member or property)
            var initialValue = ((ReadWriteNode<T>) targetNode).GetValue();
            if (initialValue.HasValue)
            {
                currentValue.NewValue(initialValue.Value);
            }
        }

        // TODO If the type is always formula, why do we call that a "ReadOnlyNodeInfo"? It's quite confusing to have an abstract name for something which can actually be only one thing. 
        // Also exposing a type like that means that there is a switch somewhere, which should ideally be replaced by a polymorphic call (code small?)
        public NodeType Type { get { return NodeType.Formula; } }

        public string FullPath { get; private set; }

        public ReevaluationResult Reevaluate()
        {
            ValueChanged();
            // Formulas do not report errors,
            // anything that relies on this formula will report the error

            // TODO I'm not sure why this is the case (formula does not report error), if a formula throws, the formula should return ReevaluationResult.Error 
            // (the instrumentation fire an event for this not with an invalid reeval result otherwise, and we get an invalid graph)
            
            return ReevaluationResult.Changed;
        }

        public void ValueChanged()
        {
            try
            {
                // TODO: I don't understand what this is doing: what does it mean to track changes of formulas? is there cases where we don't want that?
                if (shouldTrackChanges && currentValue.HasValue)
                    nodeRepository.RemoveLookup(currentValue.Value);

                // this is the current value for this formula
                // TODO: Jake, what do we do here if the current value contains an exception??
                var currentVal = currentValue.HasValue ? currentValue.Value : default(T);
                // evaluate this formula, passing the current value
                var newValue = getValue(currentVal);
                // store the new value as current value
                currentValue.NewValue(newValue);

                if (shouldTrackChanges && currentValue.HasValue)
                    nodeRepository.AddLookup(currentValue.Value, this);
            }
            catch (Exception ex)
            {
                currentValue.CouldNotCalculate(ex);
            }
        }

        public bool PathMatches(string pathToChangedValue)
        {
            return pathFromParent == pathToChangedValue;
        }

        IMaybe IValueSource.GetValue()
        {
            return GetValue();
        }

        protected bool Equals(ReadOnlyNodeInfo<T> other)
        {
            // is that enough to identify a node?
            return string.Equals(FullPath, other.FullPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ReadOnlyNodeInfo<T>) obj);
        }

        public override int GetHashCode()
        {
            return (FullPath != null ? FullPath.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return FullPath;
        }
    }
}