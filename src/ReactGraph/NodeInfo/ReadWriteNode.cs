using System;

namespace ReactGraph.NodeInfo
{
    class ReadWriteNode<T> : ITakeValue<T>, IValueSource<T>
    {
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly Func<T> getValue;
        readonly Action<T> setValue;
        IValueSource<T> valueSource;
        Action<Exception> exceptionHandler;

        public ReadWriteNode(Func<T> getValue, Action<T> setValue, string path)
        {
            Path = path;
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

        public override string ToString()
        {
            return Path;
        }

        public NodeType Type { get { return NodeType.Member; } }

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
            try
            {
                currentValue.NewValue(getValue());
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
    }
}