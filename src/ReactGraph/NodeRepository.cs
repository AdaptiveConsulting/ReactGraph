using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ReactGraph.Internals;

namespace ReactGraph
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

        public PropertyNodeInfo<T> GetOrCreate<T>(object rootValue, T parentInstance, PropertyInfo propertyInfo, MemberExpression propertyExpression)
        {
            var sourceKey = Tuple.Create<object, string>(parentInstance, propertyInfo.Name);
            if (!nodeLookup.ContainsKey(sourceKey))
            {
                var propertyNodeInfo = new PropertyNodeInfo<T>(rootValue, parentInstance, propertyInfo, propertyExpression, GetStrategies(parentInstance));
                nodeLookup.Add(sourceKey, propertyNodeInfo);
                return propertyNodeInfo;
            }

            return (PropertyNodeInfo<T>) nodeLookup[sourceKey];
        }

        private INotificationStrategy[] GetStrategies(object parentInstance)
        {
            return notificationStrategies
                .Where(notificationStrategy => notificationStrategy.AppliesTo(parentInstance))
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
    }
}