using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Internals
{
    public class PropertyNodeInfo<T> : INodeInfo
    {
        private readonly Notifications notificationStrategies;
        private readonly Func<T> getValue;
        private readonly string path;
        private FormulaExpressionInfo<T> formula;
        private T currentValue;

        /// <summary>
        /// Represents a dependency
        /// </summary>
        /// <param name="rootInstance">The root instance of the expression, i.e viewmodel in viewModel.Foo.Bar</param>
        /// <param name="parentInstance">The current parent instance of the expression, i.e Foo in viewModel.Foo.Bar</param>
        /// <param name="propertyInfo">Property info for Bar in foo.Bar</param>
        /// <param name="propertyExpression"></param>
        public PropertyNodeInfo(
            object rootInstance, 
            object parentInstance, 
            PropertyInfo propertyInfo, 
            MemberExpression propertyExpression,
            Notifications notificationStrategies)
        {
            this.notificationStrategies = notificationStrategies;
            getValue = () =>
            {
                if (parentInstance == null)
                    return default(T);
                return (T) propertyInfo.GetValue(parentInstance, null);
            };
            RootInstance = rootInstance;
            PropertyInfo = propertyInfo;
            PropertyExpression = propertyExpression;
            path = propertyExpression.ToString();
            ParentInstance = parentInstance;
        }

        public object RootInstance { get; private set; }

        public PropertyInfo PropertyInfo { get; private set; }

        public MemberExpression PropertyExpression { get; private set; }

        public object ParentInstance { get; set; }

        public override string ToString()
        {
            return path;
        }

        protected bool Equals(PropertyNodeInfo<T> other)
        {
            return string.Equals(path, other.path) && Equals(RootInstance, other.RootInstance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PropertyNodeInfo<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((path != null ? path.GetHashCode() : 0)*397) ^ (RootInstance != null ? RootInstance.GetHashCode() : 0);
            }
        }

        public static bool operator ==(PropertyNodeInfo<T> left, PropertyNodeInfo<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PropertyNodeInfo<T> left, PropertyNodeInfo<T> right)
        {
            return !Equals(left, right);
        }

        public void SetPropertySource(FormulaExpressionInfo<T> formulaNode)
        {
            formula = formulaNode;
        }

        public void Reevaluate()
        {
            if (formula != null)
            {
                currentValue = formula.GetValue();
                PropertyInfo.SetValue(ParentInstance, currentValue, null);
            }
        }

        public void ValueChanged()
        {
            // TODO Move this to pass the notification strategy into the node at construction time
            notificationStrategies.ForgetInstance(currentValue);
            currentValue = getValue();
            notificationStrategies.TrackInstanceIfNeeded(currentValue);
        }
    }
}