using System;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    class WriteOnlyNode<T> : ITakeValue<T>
    {
        readonly Action<T> setValue;
        IValueSource<T> valueSource;
        Action<Exception> exceptionHandler;

        public WriteOnlyNode(Action<T> setValue, string path)
        {
            this.setValue = setValue;
            Path = path;
        }

        public string Path { get; private set; }

        public void SetSource(IValueSource<T> sourceNode, Action<Exception> errorHandler)
        {
            if (valueSource != null)
                throw new InvalidOperationException(string.Format("{0} already has a source associated with it", Path));

            valueSource = sourceNode;
            exceptionHandler = errorHandler;
        }

        public NodeType Type { get { return NodeType.Action; } }

        public ReevaluationResult Reevaluate()
        {
            if (valueSource != null)
            {
                ValueChanged();
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

        public void ValueChanged()
        {
        }

        public override string ToString()
        {
            return Path;
        }
    }
}