using System;

namespace ReactGraph.NodeInfo
{
    class ReadOnlyNodeInfo<T> : IValueSource<T>
    {
        readonly Maybe<T> currentValue = new Maybe<T>();
        // would be good to not have this depdendency on the repoisitory here
        readonly NodeRepository nodeRepository;
        readonly string pathFromParent;
        readonly Func<T, T> getValue;
        readonly Action<Exception> onError;
        readonly NodeType type;
        bool shouldTrackChanges;

        public ReadOnlyNodeInfo(Func<T, T> getValue, string fullPath, string pathFromParent, NodeRepository nodeRepository, bool shouldTrackChanges, Action<Exception> onError, NodeType type)
        {
            FullPath = fullPath;
            this.getValue = getValue;
            this.pathFromParent = pathFromParent;
            this.nodeRepository = nodeRepository;
            this.shouldTrackChanges = shouldTrackChanges;
            this.onError = onError;
            this.type = type;

            if (shouldTrackChanges)
                UnderlyingValueHasBeenChanged();
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

        public void TrackChanges()
        {
            // I don't understand what is the purpose of this. WHat does it mean to "track" an expression?
            shouldTrackChanges = true;
            UnderlyingValueHasBeenChanged();
        }

        public void SetTarget(ITakeValue<T> targetNode, Maybe<T> initialValue)
        {
            if (initialValue.HasValue)
            {
                currentValue.NewValue(initialValue.Value);
            }
        }

        public NodeType VisualisationNodeType { get { return type; } }

        public string FullPath { get; private set; }

        public virtual ReevaluationResult Reevaluate()
        {
            return ReevaluateCurrentValue();
        }

        public void UnderlyingValueHasBeenChanged()
        {
            ReevaluateCurrentValue();
        }

        ReevaluationResult ReevaluateCurrentValue()
        {
            try
            {
                if (currentValue.HasValue)
                {
                    var currentVal = currentValue.Value;
                    // evaluate this formula, passing the current value
                    var newValue = getValue(currentVal);

                    // store the new value as current value
                    currentValue.NewValue(newValue);
                    // TODO Don't set and return NoChange when value has not changed

                    if (shouldTrackChanges)
                    {
                        nodeRepository.RemoveLookup(currentVal);
                        nodeRepository.AddLookup(newValue, this);
                    }
                }
                else
                {
                    // evaluate this formula, passing the default value of the type
                    // If the previous value was an error, we still need to pass default(T) otherwise it would stay errored forever
                    var newValue = getValue(default(T));

                    // store the new value as current value
                    currentValue.NewValue(newValue);

                    if (shouldTrackChanges && currentValue.HasValue)
                        nodeRepository.AddLookup(currentValue.Value, this);
                }
            }
            catch (Exception ex)
            {
                currentValue.CouldNotCalculate(ex);
                onError(ex);
                return ReevaluationResult.Error;
            }
            return ReevaluationResult.Changed;
        }

        public bool PathMatches(string pathToChangedValue)
        {
            return pathFromParent == pathToChangedValue;
        }

        protected bool Equals(ReadOnlyNodeInfo<T> other)
        {
            // TODO is that enough to identify a node?
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