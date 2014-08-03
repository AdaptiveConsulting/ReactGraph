using System;
using System.Linq.Expressions;
using System.Reflection;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Construction
{
    class MemberDependencyDescriptor<T> : DependencyDescriptor<T>
    {
        readonly MemberExpression memberExpression;
        readonly MemberInfo memberInfo;
        readonly Func<T> getValue;
        readonly Action<T> setValue;
        readonly Type memberType;
        readonly string key;
        readonly bool isReadOnly;

        public MemberDependencyDescriptor(object rootValue, object parentInstance, FieldInfo fieldInfo, MemberExpression memberExpression)
        {
            RootInstance = rootValue;
            ParentInstance = parentInstance;
            this.memberExpression = memberExpression;
            memberInfo = fieldInfo;
            key = fieldInfo.Name;
            memberType = fieldInfo.FieldType;
            getValue = () =>
            {
                if (parentInstance == null)
                    return default(T);
                return (T)fieldInfo.GetValue(parentInstance);
            };
            setValue = o => fieldInfo.SetValue(parentInstance, o);
            isReadOnly = fieldInfo.IsInitOnly;
        }

        public MemberDependencyDescriptor(object rootValue, object parentInstance, PropertyInfo propertyInfo, MemberExpression memberExpression)
        {
            RootInstance = rootValue;
            ParentInstance = parentInstance;
            this.memberExpression = memberExpression;
            memberInfo = propertyInfo;
            key = propertyInfo.Name; 
            memberType = propertyInfo.PropertyType;
            getValue = () =>
            {
                if (parentInstance == null)
                    return default(T);
                return (T)propertyInfo.GetValue(parentInstance, null);
            };
            setValue = o => propertyInfo.SetValue(parentInstance, o, null);
            isReadOnly = !propertyInfo.CanWrite;
        }

        public override INodeInfo GetOrCreateNodeInfo(NodeRepository repo)
        {
            return GetOrCreateWritableNodeInfo(repo);
        }

        public override IWritableNodeInfo<T> GetOrCreateWritableNodeInfo(NodeRepository repo)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Target property should be writable");

            if (repo.Contains(ParentInstance, key))
                return (IWritableNodeInfo<T>) repo.Get(ParentInstance, key);

            var strategies = repo.GetStrategies(memberType);
            //TODO need a better place for this..
            foreach (var notificationStrategy in repo.GetStrategies(RootInstance.GetType()))
            {
                notificationStrategy.Track(RootInstance);
            }
            var memberNodeInfo = new MemberNodeInfo<T>(
                RootInstance, ParentInstance, memberExpression,
                strategies, MemberInfo, getValue,
                setValue, repo);
            repo.AddLookup(ParentInstance, key, memberNodeInfo);
            return memberNodeInfo;
        }

        public override string Key
        {
            get { return key; }
        }

        public override object GetValue()
        {
            return getValue();
        }

        public override bool IsReadOnly
        {
            get { return isReadOnly; }
        }

        public MemberInfo MemberInfo
        {
            get { return memberInfo; }
        }
    }
}