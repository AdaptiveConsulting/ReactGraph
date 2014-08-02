using System;
using System.Linq.Expressions;

namespace ReactGraph.Internals
{
    class FormulaExpressionInfo<T> : INodeInfo, IValueSource<T>
    {
        private readonly PropertyNodeInfo<T>[] dependencies;
        private readonly Func<T> getValue;
        private T currentValue;

        public FormulaExpressionInfo(Expression<Func<T>> formula, PropertyNodeInfo<T>[] dependencies, object rootInstance)
        {
            this.dependencies = dependencies;
            Key = formula.Name;
            RootInstance = rootInstance;
            getValue = formula.Compile();
        }

        public string Key { get; private set; }

        public INodeInfo[] Dependencies
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