using System;

namespace ReactGraph.NodeInfo
{
    class WritableNodeInfo<T> : IWritableNodeInfo<T>
    {
        readonly string label;
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly Func<T> getValue;
        readonly Action<T> setValue;
        IValueSource<T> formula;
        Action<Exception> exceptionHandler;

        public WritableNodeInfo(
            Func<T> getValue,
            Action<T> setValue,
            string label)
        {
            this.setValue = setValue;
            this.label = label;
            this.getValue = getValue;
            ValueChanged();
        }

        public void SetSource(IValueSource<T> formulaNode, Action<Exception> errorHandler)
        {
            if (formula != null)
                throw new InvalidOperationException(string.Format("{0} already has a formula associated with it", label));

            formula = formulaNode;
            exceptionHandler = errorHandler;
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

        public override string ToString()
        {
            return label;
        }

        IMaybe IValueSource.GetValue()
        {
            return GetValue();
        }

        public NodeType Type { get { return NodeType.Member; } }

        public ReevaluationResult Reevaluate()
        {
            if (formula != null)
            {
                ValueChanged();
                var value = formula.GetValue();
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
    }
}