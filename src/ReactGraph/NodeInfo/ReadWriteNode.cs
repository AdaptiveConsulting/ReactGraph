using System;

namespace ReactGraph.NodeInfo
{
    class ReadWriteNode<T> : ReadOnlyNodeInfo<T>, ITakeValue<T>
    {
        readonly Action<T> setValue;
        IValueSource<T> valueSource;

        public ReadWriteNode(Func<T> getValue, Action<T> setValue, string fullPath, string pathFromParent, NodeType visualisationNodeType, NodeRepository nodeRepository, bool shouldTrackChanges, Action<Exception> exceptionHandler) :
            base(_ => getValue(), fullPath, pathFromParent, nodeRepository, shouldTrackChanges, exceptionHandler, visualisationNodeType)
        {
            this.setValue = setValue;
        }

        public void SetSource(IValueSource<T> sourceNode)
        {
            if (valueSource != null)
                throw new InvalidOperationException(string.Format("{0} already has a source associated with it", FullPath));

            valueSource = sourceNode;
        }

        public override ReevaluationResult Reevaluate()
        {
            if (valueSource != null)
            {
                var value = valueSource.GetValue();
                if (value.HasValue)
                {
                    setValue(value.Value);

                    return base.Reevaluate();
                }
            }

            return ReevaluationResult.NoChange;
        }

        public override string ToString()
        {
            return FullPath;
        }

        protected bool Equals(ReadWriteNode<T> other)
        {
            // TODO is that enough to guarantee identity?
            return string.Equals(FullPath, other.FullPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ReadWriteNode<T>) obj);
        }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }
    }
}