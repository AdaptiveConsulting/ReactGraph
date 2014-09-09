using System;

namespace ReactGraph.NodeInfo
{
    class ReadWriteNode<T> : ITakeValue<T>, IValueSource<T>
    {
        // TODO why do we cache the current value? Coulnd't we read from the corresponding member directly when needed?
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly NodeRepository nodeRepository;
        bool shouldTrackChanges;

        // TODO rename to indicate that this get and set to the corresponding member / property?
        readonly Action<T> setValue;
        readonly Func<T> getValue;
        readonly NodeType visualisationNodeType;

        // TODO isn't that always a formula? why do we call it valueSource? This node is related to 2 things: a property/member and a formula, source could apply to both.
        IValueSource<T> valueSource;

        // TODO why is the exception handler on a ReadWriteNode?
        Action<Exception> exceptionHandler;

        public ReadWriteNode(Func<T> getValue, Action<T> setValue, string path, NodeType visualisationNodeType, NodeRepository nodeRepository, bool shouldTrackChanges)
        {
            Path = path;
            // TODO isn't type always "member"? 
            this.visualisationNodeType = visualisationNodeType;
            this.nodeRepository = nodeRepository;

            this.shouldTrackChanges = shouldTrackChanges;
            this.setValue = setValue;
            this.getValue = getValue;
            ValueChanged();
        }

        public string Path { get; private set; }

        public void SetSource(IValueSource<T> sourceNode, Action<Exception> errorHandler)
        {
            if (valueSource != null)
                throw new InvalidOperationException(string.Format("{0} already has a source associated with it", Path));

            valueSource = sourceNode;
            exceptionHandler = errorHandler;
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

        public void TrackChanges()
        {
            shouldTrackChanges = true;
        }

        public void SetTarget(ITakeValue<T> targetNode)
        {
            // no op, we need this only for ReadOnlyNodes (ie. formulas)
        }

        public override string ToString()
        {
            return Path;
        }

        // TODO always Member?
        public NodeType VisualisationNodeType { get { return visualisationNodeType; } }

        // TODO have you considered putting that logic in the depdendency engine?
        // it looks to me that the depdendency engine would have evaluated the source formula just before, so it could have the result and set it on this node, this would remove the need to have valueSource
        public ReevaluationResult Reevaluate()
        {
            if (valueSource != null)
            {
                var value = valueSource.GetValue();
                if (value.HasValue)
                {
                    // TODO Don't set and return NoChange when value has not changed
                    setValue(value.Value);

                    // TODO why do we need to do that? 
                    ValueChanged();
                    return ReevaluationResult.Changed;
                }

                exceptionHandler(value.Exception);
                return ReevaluationResult.Error;
            }

            // TODO can a member be re-evaluated and have no value source? Should we throw here?

            return ReevaluationResult.NoChange;
        }

        public void ValueChanged()
        {
            try
            {
                // TODO that's for the INotifyPropertyChange tracking I guess? we unhook and rehook even when value has not changed?
                if (shouldTrackChanges && currentValue.HasValue)
                    nodeRepository.RemoveLookup(currentValue.Value);

                // read the value from the field / property
                var newValue = getValue();
                // store the new value
                currentValue.NewValue(newValue);

                if (shouldTrackChanges && currentValue.HasValue)
                    nodeRepository.AddLookup(currentValue.Value, this);
            }
            catch (Exception ex)
            {
                currentValue.CouldNotCalculate(ex);
            }
        }

        protected bool Equals(ReadWriteNode<T> other)
        {
            // TODO is that enough to guarantee identity?
            return string.Equals(Path, other.Path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReadWriteNode<T>) obj);
        }

        public override int GetHashCode()
        {
            // TODO can the path be null?
            return (Path != null ? Path.GetHashCode() : 0);
        }

        IMaybe IValueSource.GetValue()
        {
            // TODO why do we need a non generic version?
            return GetValue();
        }
    }
}