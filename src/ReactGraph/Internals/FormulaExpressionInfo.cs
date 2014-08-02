using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ReactGraph.Internals
{
    class FormulaExpressionInfo<T> : INodeInfo, IValueSource<T>
    {
        readonly Expression<Func<T>> formula;
        private readonly Func<T> getValue;
        private T currentValue;

        public FormulaExpressionInfo(Expression<Func<T>> formula)
        {
            this.formula = formula;
            Key = formula.Name;
            getValue = formula.Compile();
            Dependencies = new List<INodeInfo>();
        }

        public string Key { get; private set; }

        public List<INodeInfo> Dependencies { get; private set; }

        public INodeInfo ReduceIfPossible()
        {
            if (Dependencies.Count == 1 && (formula.Body is MemberExpression || formula.Body is MethodCallExpression))
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

        public object RootInstance { get; private set; }

        public object ParentInstance { get; set; }

        bool Equals(FormulaExpressionInfo<T> other)
        {
            return string.Equals(Key, other.Key) && Equals(RootInstance, other.RootInstance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FormulaExpressionInfo<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0)*397) ^ (RootInstance != null ? RootInstance.GetHashCode() : 0);
            }
        }

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
    }
}