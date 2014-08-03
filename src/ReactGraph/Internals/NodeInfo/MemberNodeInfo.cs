using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ReactGraph.Internals.Notification;
using ReactGraph.Properties;

namespace ReactGraph.Internals.NodeInfo
{
    class MemberNodeInfo<T> : INodeInfo, IValueSink<T>, IValueSource<T>
    {
        readonly NodeRepository nodeRepository;

        readonly INotificationStrategy[] notificationStrategies;
        readonly Func<T> getValue;
        readonly Action<object> setValue;
        readonly string path;
        IValueSource<T> formula;
        T currentValue;
        object parentInstance;

        private MemberNodeInfo(object rootInstance, object parentInstance,
            MemberExpression propertyExpression,
            INotificationStrategy[] notificationStrategies,
            MemberInfo memberInfo,
            Func<T> getValue,
            Action<object> setValue, NodeRepository nodeRepository)
        {
            this.notificationStrategies = notificationStrategies;
            this.setValue = setValue;
            this.nodeRepository = nodeRepository;
            this.getValue = getValue;
            RootInstance = rootInstance;
            MemberInfo = memberInfo;
            PropertyExpression = propertyExpression;
            path = propertyExpression.ToString();
            ParentInstance = parentInstance;
            Dependencies = new List<INodeInfo>();
            ValueChanged();
        }

        public MemberNodeInfo(
            object rootInstance, object parentInstance,
            PropertyInfo propertyInfo, MemberExpression propertyExpression,
            INotificationStrategy[] notificationStrategies, NodeRepository nodeRepository) :
            this(rootInstance, parentInstance, propertyExpression, notificationStrategies,
            propertyInfo, () =>
            {
                if (parentInstance == null)
                    return default(T);
                return (T)propertyInfo.GetValue(parentInstance, null);
            },
            o => propertyInfo.SetValue(parentInstance, o, null), nodeRepository)
        {
        }

        [UsedImplicitly]
        public MemberNodeInfo(
            object rootInstance, object parentInstance,
            FieldInfo fieldInfo, MemberExpression propertyExpression,
            INotificationStrategy[] notificationStrategies, NodeRepository nodeRepository) :
            this(rootInstance, parentInstance, propertyExpression, notificationStrategies,
            fieldInfo, () =>
            {
                if (parentInstance == null)
                    return default(T);
                return (T)fieldInfo.GetValue(parentInstance);
            },
            o => fieldInfo.SetValue(parentInstance, o), nodeRepository)
        {
        }

        public object RootInstance { get; set; }

        public MemberInfo MemberInfo { get; private set; }

        MemberExpression PropertyExpression { get; set; }

        public object ParentInstance
        {
            get { return parentInstance; }
            set
            {
                nodeRepository.RemoveLookup(parentInstance, MemberInfo.Name);
                parentInstance = value;
                nodeRepository.AddLookup(parentInstance, MemberInfo.Name, this);
            }
        }

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
            return ExpressionStringBuilder.ToString(PropertyExpression);
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
                setValue(formula.GetValue());
            }
        }

        public void ValueChanged()
        {
            foreach (var notificationStrategy in notificationStrategies)
                notificationStrategy.Untrack(currentValue);
            nodeRepository.RemoveLookup(currentValue, null);

            currentValue = getValue();

            nodeRepository.AddLookup(currentValue, null, this);
            foreach (var notificationStrategy in notificationStrategies)
                notificationStrategy.Track(currentValue);
        }
    }
}