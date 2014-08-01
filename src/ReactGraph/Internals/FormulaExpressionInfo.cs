using System;
using System.Linq.Expressions;

namespace ReactGraph.Internals
{
    public class FormulaExpressionInfo<T> : INodeInfo
    {
        private readonly PropertyNodeInfo<T>[] dependencies;
        private Func<T> getValue;
        private T currentValue;

        public FormulaExpressionInfo(Expression<Func<T>> formula, PropertyNodeInfo<T>[] dependencies)
        {
            this.dependencies = dependencies;
            getValue = formula.Compile();
        }

        public PropertyNodeInfo<T>[] Dependencies
        {
            get { return dependencies; }
        }

        public T GetValue()
        {
            return currentValue;
        }

        public void Reevaluate()
        {
            currentValue = getValue();
        }

        public void ValueChanged()
        {
            currentValue = getValue();
        }
    }
}