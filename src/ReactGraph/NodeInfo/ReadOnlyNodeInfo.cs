using System;

namespace ReactGraph.NodeInfo
{
    class ReadOnlyNodeInfo<T> : IValueSource<T>
    {
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly Func<T> getValue;
        readonly string label;

        public ReadOnlyNodeInfo(Func<T> getValue, string label)
        {
            this.label = label;
            this.getValue = getValue;
            ValueChanged();
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

        public NodeType Type { get { return NodeType.Formula; } }

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
                currentValue.NewValue(getValue());
            }
            catch (Exception ex)
            {
                currentValue.CouldNotCalculate(ex);
            }
        }

        public override string ToString()
        {
            return label;
        }
    }
}