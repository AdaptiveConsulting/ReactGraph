using System;

namespace ReactGraph.NodeInfo
{
    class ReadOnlyNodeInfo<T> : IValueSource<T>
    {
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly NodeRepository nodeRepository;
        readonly Func<T, T> getValue;
        bool shouldTrackChanges;

        public ReadOnlyNodeInfo(Func<T, T> getValue, string path, NodeRepository nodeRepository, bool shouldTrackChanges)
        {
            Path = path;
            this.getValue = getValue;
            this.nodeRepository = nodeRepository;
            this.shouldTrackChanges = shouldTrackChanges;
            ValueChanged();
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

        public void TrackChanges()
        {
            shouldTrackChanges = true;
        }

        public NodeType Type { get { return NodeType.Formula; } }

        public string Path { get; private set; }

        public ReevaluationResult Reevaluate()
        {
            ValueChanged();
            // Formulas do not report errors,
            // anything that relies on this formula will report the error
            return ReevaluationResult.Changed;
        }

        public void ValueChanged()
        {
            try
            {
                if (shouldTrackChanges && currentValue.HasValue)
                    nodeRepository.RemoveLookup(currentValue.Value);
                currentValue.NewValue(getValue(currentValue.HasValue ? currentValue.Value : default(T)));
                if (shouldTrackChanges && currentValue.HasValue)
                    nodeRepository.AddLookup(currentValue.Value, this);
            }
            catch (Exception ex)
            {
                currentValue.CouldNotCalculate(ex);
            }
        }

        IMaybe IValueSource.GetValue()
        {
            return GetValue();
        }

        protected bool Equals(ReadOnlyNodeInfo<T> other)
        {
            return string.Equals(Path, other.Path);
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
            return (Path != null ? Path.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}