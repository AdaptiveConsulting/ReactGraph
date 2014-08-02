using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Internals
{
    class MemberNodeInfo<T> : INodeInfo, IValueSink<T>, IValueSource<T>
    {
        private readonly INotificationStrategy[] notificationStrategies;
        private readonly Func<T> getValue;
        private readonly string path;
        private IValueSource<T> formula;
        private T currentValue;
        Action<object> setValue;

        /// <summary>
        /// Represents a dependency
        /// </summary>
        /// <param name="rootInstance">The root instance of the expression, i.e viewmodel in viewModel.Foo.Bar</param>
        /// <param name="parentInstance">The current parent instance of the expression, i.e Foo in viewModel.Foo.Bar</param>
        /// <param name="propertyInfo">Property info for Bar in foo.Bar</param>
        /// <param name="propertyExpression"></param>
        /// <param name="notificationStrategies"></param>
        public MemberNodeInfo(
            object rootInstance, object parentInstance, 
            MemberInfo memberInfo, MemberExpression propertyExpression, 
            INotificationStrategy[] notificationStrategies)
        {
            this.notificationStrategies = notificationStrategies;
            getValue = () =>
            {
                if (parentInstance == null)
                    return default(T);
                var info = memberInfo as PropertyInfo;
                if (info != null)
                    return (T)info.GetValue(parentInstance, null);
                var fieldInfo = memberInfo as FieldInfo;
                if (fieldInfo != null)
                    return (T)fieldInfo.GetValue(parentInstance);
                return default(T);
            };
            setValue = o =>
            {
                var info = memberInfo as PropertyInfo;
                if (info != null)
                    info.SetValue(parentInstance, o, null);
                var fieldInfo = memberInfo as FieldInfo;
                if (fieldInfo != null)
                    fieldInfo.SetValue(parentInstance, o);
            };
            RootInstance = rootInstance;
            MemberInfo = memberInfo;
            PropertyExpression = propertyExpression;
            path = propertyExpression.ToString();
            ParentInstance = parentInstance;
            Dependencies = new List<INodeInfo>();
        }

        public object RootInstance { get; private set; }

        public MemberInfo MemberInfo { get; private set; }

        public MemberExpression PropertyExpression { get; private set; }

        public object ParentInstance { get; set; }

        public string Key { get; private set; }

        public List<INodeInfo> Dependencies { get; private set; }

        public INodeInfo ReduceIfPossible()
        {
            return this;
        }

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

        bool Equals(MemberNodeInfo<T> other)
        {
            return string.Equals(path, other.path) && Equals(RootInstance, other.RootInstance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MemberNodeInfo<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((path != null ? path.GetHashCode() : 0) * 397) ^ (RootInstance != null ? RootInstance.GetHashCode() : 0);
            }
        }

        object IValueSource.GetValue()
        {
            return GetValue();
        }

        public static bool operator ==(MemberNodeInfo<T> left, MemberNodeInfo<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MemberNodeInfo<T> left, MemberNodeInfo<T> right)
        {
            return !Equals(left, right);
        }

        public void Reevaluate()
        {
            if (formula != null)
            {
                ValueChanged();
                setValue(currentValue);
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