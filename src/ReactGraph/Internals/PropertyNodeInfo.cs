using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Internals
{
    class PropertyNodeInfo<T> : INodeInfo, IValueSink<T>, IValueSource<T>
    {
        private readonly INotificationStrategy[] notificationStrategies;
        private readonly Func<T> getValue;
        private readonly string path;
        private IValueSource<T> formula;
        private T currentValue;

        /// <summary>
        /// Represents a dependency
        /// </summary>
        /// <param name="rootInstance">The root instance of the expression, i.e viewmodel in viewModel.Foo.Bar</param>
        /// <param name="parentInstance">The current parent instance of the expression, i.e Foo in viewModel.Foo.Bar</param>
        /// <param name="propertyInfo">Property info for Bar in foo.Bar</param>
        /// <param name="propertyExpression"></param>
        /// <param name="notificationStrategies"></param>
        public PropertyNodeInfo(
            object rootInstance, object parentInstance, 
            PropertyInfo propertyInfo, MemberExpression propertyExpression, 
            INotificationStrategy[] notificationStrategies)
        {
            this.notificationStrategies = notificationStrategies;
            getValue = () =>
            {
                if (parentInstance == null)
                    return default(T);
                return (T)propertyInfo.GetValue(parentInstance, null);
            };
            RootInstance = rootInstance;
            PropertyInfo = propertyInfo;
            PropertyExpression = propertyExpression;
            path = propertyExpression.ToString();
            ParentInstance = parentInstance;
            Dependencies = new List<INodeInfo>();
        }

        public object RootInstance { get; private set; }

        public PropertyInfo PropertyInfo { get; private set; }

        public MemberExpression PropertyExpression { get; private set; }

        public object ParentInstance { get; set; }

        public string Key { get; private set; }

        public List<INodeInfo> Dependencies { get; private set; }

        public void SetSource(IValueSource<T> formulaNode)
        {
            formula = formulaNode;
        }

        public T GetValue()
        {
            return currentValue;
        }

        public override string ToString()
        {
            return path;
        }

        bool Equals(PropertyNodeInfo<T> other)
        {
            return string.Equals(path, other.path) && Equals(RootInstance, other.RootInstance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PropertyNodeInfo<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((path != null ? path.GetHashCode() : 0) * 397) ^ (RootInstance != null ? RootInstance.GetHashCode() : 0);
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

        public void Reevaluate()
        {
            if (formula != null)
            {
                ValueChanged();
                PropertyInfo.SetValue(ParentInstance, currentValue, null);
            }
        }

        public void ValueChanged()
        {
            foreach (var notificationStrategy in notificationStrategies)
                notificationStrategy.Untrack(currentValue);
            currentValue = getValue();
            foreach (var dependency in Dependencies)
            {
                dependency.ParentInstance = currentValue;
            }
            foreach (var notificationStrategy in notificationStrategies)
                notificationStrategy.Track(currentValue);
        }
    }
}