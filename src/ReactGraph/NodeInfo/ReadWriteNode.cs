using System;

namespace ReactGraph.NodeInfo
{
    class ReadWriteNode<T> : ITakeValue<T>, IValueSource<T>
    {
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly NodeRepository nodeRepository;
        bool shouldTrackChanges;
        readonly Action<T> setValue;
        readonly Func<T> getValue;
        readonly NodeType type;
        IValueSource<T> valueSource;
        Action<Exception> exceptionHandler;

        public ReadWriteNode(Func<T> getValue, Action<T> setValue, string path, NodeType type, NodeRepository nodeRepository, bool shouldTrackChanges)
        {
            Path = path;
            this.type = type;
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

        public override string ToString()
        {
            return Path;
        }

        public NodeType Type { get { return type; } }

        public ReevaluationResult Reevaluate()
        {
            if (valueSource != null)
            {
                var value = valueSource.GetValue();
                if (value.HasValue)
                {
                    // TODO Don't set and return NoChange when value has not changed
                    setValue(value.Value);
                    ValueChanged();
                    return ReevaluationResult.Changed;
                }

                exceptionHandler(value.Exception);
                return ReevaluationResult.Error;
            }

            return ReevaluationResult.NoChange;
        }

        public void ValueChanged()
        {
            try
            {
                if (shouldTrackChanges && currentValue.HasValue)
                    nodeRepository.RemoveLookup(currentValue.Value);
                currentValue.NewValue(getValue());
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
            return (Path != null ? Path.GetHashCode() : 0);
        }

        IMaybe IValueSource.GetValue()
        {
            return GetValue();
        }
    }
}