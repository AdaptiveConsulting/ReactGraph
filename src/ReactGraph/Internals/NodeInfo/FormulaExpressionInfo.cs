using System;
using ReactGraph.Internals.Construction;

namespace ReactGraph.Internals.NodeInfo
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

        public void Reevaluate()
        {
            ValueChanged();
        }

        public void ValueChanged()
        {
            try
            {
                currentValue.NewValue(getValue());
            }
            catch (FormulaNullReferenceException)
            {
                currentValue.ValueMissing();
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