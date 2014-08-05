using System;

namespace ReactGraph.NodeInfo
{
    class FormulaExpressionInfo<T> : INodeInfo<T>
    {
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly Func<T> getValue;
        readonly string label;

        public FormulaExpressionInfo(Func<T> execute, string label)
        {
            this.label = label;
            getValue = execute;
            ValueChanged();
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

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

        public void UpdateSubscriptions(IMaybe newParent)
        {
        }

        public override string ToString()
        {
            return label;
        }

        IMaybe IValueSource.GetValue()
        {
            return GetValue();
        }
    }
}