using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ReactGraph.Internals.Notification;

namespace ReactGraph.Internals.NodeInfo
{
    class NodeRepository
    {
        private readonly Dictionary<Tuple<object, string>, INodeInfo> nodeLookup;
        private readonly List<INotificationStrategy> notificationStrategies;

        public NodeRepository(DependencyEngine dependencyEngine)
        {
            nodeLookup = new Dictionary<Tuple<object, string>, INodeInfo>();
            notificationStrategies = new List<INotificationStrategy>
            {
                new NotifyPropertyChangedStrategy(dependencyEngine)
            };
        }

        public INodeInfo GetOrCreate(object rootValue, object parentInstance, MemberInfo memberInfo, MemberExpression propertyExpression)
        {
            var sourceKey = Tuple.Create(rootValue, memberInfo.Name);
            if (!nodeLookup.ContainsKey(sourceKey))
            {
                var propertyInfo = memberInfo as PropertyInfo;
                INodeInfo propertyNodeInfo;
                if (propertyInfo != null)
                {
                    var type = typeof(MemberNodeInfo<>).MakeGenericType(propertyInfo.PropertyType);
                    propertyNodeInfo = (INodeInfo) Activator.CreateInstance(
                        type, rootValue, parentInstance, 
                        propertyInfo, propertyExpression,
                        GetStrategies(propertyInfo.PropertyType),
                        this);
                }
                else
                {
                    var fieldInfo = ((FieldInfo)memberInfo);
                    var type = typeof(MemberNodeInfo<>).MakeGenericType(fieldInfo.FieldType);
                    propertyNodeInfo = (INodeInfo)Activator.CreateInstance(
                        type, rootValue, parentInstance,
                        fieldInfo, propertyExpression,
                        GetStrategies(fieldInfo.FieldType),
                        this);
                }

                foreach (var notificationStrategy in GetStrategies(rootValue.GetType()))
                {
                    notificationStrategy.Track(rootValue);
                }
                return propertyNodeInfo;
            }

            return nodeLookup[sourceKey];
        }

        public INodeInfo GetOrCreate<T>(LambdaExpression methodExpression)
        {
            var sourceKey = Tuple.Create<object, string>(null, methodExpression.ToString());
            if (!nodeLookup.ContainsKey(sourceKey))
            {
                var propertyNodeInfo = new FormulaExpressionInfo<T>((Expression<Func<T>>) methodExpression);
                nodeLookup.Add(sourceKey, propertyNodeInfo);
                return propertyNodeInfo;
            }

            return nodeLookup[sourceKey];
        }

        private INotificationStrategy[] GetStrategies(Type type)
        {
            return notificationStrategies
                .Where(notificationStrategy => notificationStrategy.AppliesTo(type))
                .ToArray();
        }

        public bool Contains(object instance, string key)
        {
            return nodeLookup.ContainsKey(Tuple.Create(instance, key));
        }

        public INodeInfo Get(object instance, string key)
        {
            return nodeLookup[Tuple.Create(instance, key)];
        }

        public void RemoveLookup(object instance, string key)
        {
            var tuple = Tuple.Create(instance, key);
            if (nodeLookup.ContainsKey(tuple))
                nodeLookup.Remove(tuple);
        }

        public void AddLookup(object instance, string key, INodeInfo nodeInfo)
        {
            var tuple = Tuple.Create(instance, key);
            if (!nodeLookup.ContainsKey(tuple))
                nodeLookup.Add(tuple, nodeInfo);
        }
    }
}