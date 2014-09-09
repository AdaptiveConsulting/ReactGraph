using System;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    class WriteOnlyNode<T> : ITakeValue<T>
    {
        readonly Action<Exception> exceptionHandler;
        readonly Action<T> setValue;
        IValueSource<T> valueSource;

        public WriteOnlyNode(Action<T> setValue, Action<Exception> exceptionHandler, string fullPath)
        {
            this.setValue = setValue;
            FullPath = fullPath;
            this.exceptionHandler = exceptionHandler;
        }

        public string FullPath { get; private set; }

        public void SetSource(IValueSource<T> sourceNode)
        {
            if (valueSource != null)
                throw new InvalidOperationException(string.Format("{0} already has a source associated with it", FullPath));

            valueSource = sourceNode;
        }

        public NodeType VisualisationNodeType { get { return NodeType.Action; } }

        public ReevaluationResult Reevaluate()
        {
            // TODO again, I think the engine should do that
            if (valueSource != null)
            {
                UnderlyingValueHasBeenChanged();
                var value = valueSource.GetValue();
                if (value.HasValue)
                {
                    // TODO Don't set and return NoChange when value has not changed
                    setValue(value.Value);
                    return ReevaluationResult.Changed;
                }

                exceptionHandler(value.Exception);
                return ReevaluationResult.Error;
            }

            return ReevaluationResult.NoChange;
        }

        public void UnderlyingValueHasBeenChanged()
        {
        }

        public bool PathMatches(string pathToChangedValue)
        {
            return false;
        }

        public override string ToString()
        {
            return FullPath;
        }

        protected bool Equals(WriteOnlyNode<T> other)
        {
            return string.Equals(FullPath, other.FullPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((WriteOnlyNode<T>)obj);
        }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }
    }
}