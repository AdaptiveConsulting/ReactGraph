using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Internals
{
    public class DependencyInfo
    {
        private Action reevaluateValue;
        private readonly Func<object> getValue;
        private readonly string path;

        /// <summary>
        /// Represents a dependency
        /// </summary>
        /// <param name="rootInstance">The root instance of the expression, i.e viewmodel in viewModel.Foo.Bar</param>
        /// <param name="parentInstance">The current parent instance of the expression, i.e Foo in viewModel.Foo.Bar</param>
        /// <param name="propertyInfo">Property info for Bar in foo.Bar</param>
        /// <param name="propertyExpression"></param>
        /// <param name="reevaluateValue">If part of a Bind expression, causes that expression to reevaluate</param>
        public DependencyInfo(
            object rootInstance, 
            object parentInstance, 
            PropertyInfo propertyInfo, 
            MemberExpression propertyExpression, 
            Action reevaluateValue)
        {
            //TODO Could maybe use reevaluateValue to switch instances?
            this.reevaluateValue = reevaluateValue;
            getValue = () =>
            {
                if (parentInstance == null)
                    return null;
                return propertyInfo.GetValue(parentInstance, null);
            };
            RootInstance = rootInstance;
            PropertyInfo = propertyInfo;
            PropertyExpression = propertyExpression;
            path = propertyExpression.ToString();
            this.ParentInstance = parentInstance;
        }

        public object RootInstance { get; private set; }

        public PropertyInfo PropertyInfo { get; private set; }

        public MemberExpression PropertyExpression { get; private set; }

        public object ParentInstance { get; set; }

        public void ReevalValue()
        {
            if (reevaluateValue != null)
                reevaluateValue();
        }

        public override string ToString()
        {
            return path;
        }

        protected bool Equals(DependencyInfo other)
        {
            return string.Equals(path, other.path) && Equals(RootInstance, other.RootInstance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DependencyInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((path != null ? path.GetHashCode() : 0)*397) ^ (RootInstance != null ? RootInstance.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DependencyInfo left, DependencyInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DependencyInfo left, DependencyInfo right)
        {
            return !Equals(left, right);
        }

        // TODO Should cache last value
        public object GetValue()
        {
            if (getValue == null)
                return null;
            return getValue();
        }

        public void Merge(DependencyInfo sourceVertex)
        {
            if (reevaluateValue == null)
                reevaluateValue = sourceVertex.reevaluateValue;
        }
    }
}