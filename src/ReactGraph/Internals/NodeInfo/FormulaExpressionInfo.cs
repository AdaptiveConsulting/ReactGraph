using System;

namespace ReactGraph.Internals.NodeInfo
{
    class FormulaExpressionInfo<T> : INodeInfo<T>
    {
        private readonly Func<T> getValue;
        private T currentValue;
        readonly string label;

        public FormulaExpressionInfo(Func<T> execute, string label)
        {
            this.label = label;
            var compiledFormula = execute;
            getValue = () =>
            {
                try
                {
                    return compiledFormula();
                }
                catch (NullReferenceException)
                {
                    return default(T);
                }
            };
            currentValue = getValue();
        }

        public T GetValue()
        {
            return currentValue;
        }

        public void Reevaluate()
        {
            ValueChanged();
        }

        public void ValueChanged()
        {
            currentValue = getValue();
        }

        public void UpdateSubscriptions(object newParent)
        {
        }

        object IValueSource.GetValue()
        {
            return GetValue();
        }

        public override string ToString()
        {
            return label;
        }
    }
}