using System;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    class WriteOnlyNode<T> : ITakeValue<T>
    {
        readonly Action<Exception> exceptionHandler;
        readonly Action<T> setValue;
        IValueSource<T> valueSource;

        public WriteOnlyNode(Action<T> setValue, Action<Exception> exceptionHandler, string path)
        {
            this.setValue = setValue;
            this.exceptionHandler = exceptionHandler;
            Path = path;
        }

        public string Path { get; private set; }

        public void SetSource(IValueSource<T> sourceNode)
        {
            if (valueSource != null)
                throw new InvalidOperationException(string.Format("{0} already has a source associated with it", Path));

            valueSource = sourceNode;
        }

        public NodeType Type { get { return NodeType.Action; } }

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

        public override string ToString()
        {
            return Path;
        }

        protected bool Equals(WriteOnlyNode<T> other)
        {
            return string.Equals(Path, other.Path);
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
            return Path.GetHashCode();
        }
    }
}