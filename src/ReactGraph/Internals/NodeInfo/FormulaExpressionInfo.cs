using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ReactGraph.Internals.NodeInfo
{
    class FormulaExpressionInfo<T> : INodeInfo<T> // TODO override gethashcode and equal
    {
        readonly Expression<Func<T>> formula;
        private readonly Func<T> getValue;
        private T currentValue;

        public FormulaExpressionInfo(Expression<Func<T>> formula, Func<T> execute)
        {
            this.formula = formula;
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
            Dependencies = new List<INodeInfo>();
        }

        public List<INodeInfo> Dependencies { get; private set; }

        public INodeInfo ReduceIfPossible()
        {
            if (Dependencies.Count == 1 && (formula.Body is MemberExpression))
            {
                return Dependencies.Single();
            }

            return this;
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

        public object RootInstance { get; set; }

        public object ParentInstance { get; set; }

        object IValueSource.GetValue()
        {
            return GetValue();
        }

        public static bool operator ==(FormulaExpressionInfo<T> left, FormulaExpressionInfo<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FormulaExpressionInfo<T> left, FormulaExpressionInfo<T> right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return ExpressionStringBuilder.ToString(formula);
        }
    }
}