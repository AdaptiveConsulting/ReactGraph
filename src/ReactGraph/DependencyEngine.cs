using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Internals;

namespace ReactGraph
{
    public class DependencyEngine
    {
        private readonly List<INotificationStrategy> notificationStrategies;
        private readonly Dictionary<Tuple<object, string>, DependencyInfo> nodeLookup = new Dictionary<Tuple<object, string>, DependencyInfo>();
        private readonly Dictionary<DependencyInfo, object> instancesToSwitch = new Dictionary<DependencyInfo, object>();
        private readonly Dictionary<DependencyInfo, object> leafDependencies = new Dictionary<DependencyInfo, object>();
        private readonly List<object> instancesBeingTracked = new List<object>();
        private readonly DirectedGraph<DependencyInfo> graph;
        private readonly ExpressionParser expressionParser;
        private bool isExecuting;

        public DependencyEngine()
        {
            notificationStrategies = new List<INotificationStrategy>
            {
                new NotifyPropertyChangedStrategy(this)
            };
            graph = new DirectedGraph<DependencyInfo>();
            expressionParser = new ExpressionParser();
        }

        public event Action<object, string> SettingValue = (o, s) => { };

        public void PropertyChanged(object instance, string property)
        {
            var key = Tuple.Create(instance, property);
            if (!nodeLookup.ContainsKey(key)) return;
            var node = nodeLookup[key];

            SwitchTransientInstances(node);
            UpdateLeafDependencies(node);


            if (isExecuting) return;
            try
            {
                isExecuting = true;
                var orderToReeval = graph.TopologicalSort(node);
                foreach (var vertex in orderToReeval.Skip(1))
                {
                    SettingValue(vertex.Data.RootInstance, vertex.Data.PropertyInfo.Name);
                    vertex.Data.ReevalValue();
                }
            }
            finally
            {
                isExecuting = false;
            }
        }

        private void UpdateLeafDependencies(DependencyInfo node)
        {
            if (leafDependencies.ContainsKey(node))
            {
                var currentLeaf = leafDependencies[node];
                ForgetInstance(currentLeaf);
                var instance = node.GetValue();
                TrackInstanceIfNeeded(instance);
                leafDependencies[node] = instance;
            }
        }

        private void SwitchTransientInstances(DependencyInfo node)
        {
            if (instancesToSwitch.ContainsKey(node))
            {
                var oldInstance = instancesToSwitch[node];
                ForgetInstance(oldInstance);
                var newInstance = node.GetValue();
                instancesToSwitch[node] = newInstance;
                TrackInstanceIfNeeded(newInstance);
                node.ParentInstance = newInstance;
                var lookupsToUpdate = nodeLookup.Keys.Where(k => k.Item1 == oldInstance).ToArray();
                foreach (var tuple in lookupsToUpdate)
                {
                    var newKey = Tuple.Create(newInstance, tuple.Item2);
                    if (!nodeLookup.ContainsKey(newKey))
                        nodeLookup.Add(newKey, node);
                    nodeLookup.Remove(tuple);
                }
            }
        }

        public void Bind<TProp>(Expression<Func<TProp>> targetProperty, Expression<Func<TProp>> sourceFunction)
        {
            var targetVertex = expressionParser.GetNodeInfo(targetProperty, sourceFunction);
            TrackInstanceIfNeeded(targetVertex.RootInstance);
            var sourceVertices = expressionParser.GetSourceVerticies(sourceFunction);

            ProcessNodes(sourceVertices, targetVertex, targetVertex);
        }

        public override string ToString()
        {
            return graph.ToDotLanguage("DependencyGraph");
        }

        private void ProcessNodes(DependencyInfo[] sourceVertices, DependencyInfo targetVertex, DependencyInfo originalTarget)
        {
            foreach (var sourceVertex in sourceVertices)
            {
                TrackInstanceIfNeeded(sourceVertex.RootInstance);
                AddNodes(targetVertex, sourceVertex);
                // When different the target vertex is a transient property, i.e Foo in viewModel.Foo.Bar 
                if (targetVertex != originalTarget)
                {
                    AddNodes(originalTarget, sourceVertex);
                    SwitchInstanceWhenChanged(sourceVertex);
                }
                else
                {
                    // If we are at the top level we want to listen to the values
                    TrackLeafDependency(sourceVertex);
                }

                if (sourceVertex.PropertyExpression != null)
                {
                    var parent = sourceVertex.PropertyExpression.Expression;
                    if (parent is MemberExpression)
                    {
                        var subNodes = expressionParser.GetSourceVerticies(parent);

                        ProcessNodes(subNodes, sourceVertex, originalTarget);
                    }
                }
            }
        }

        private void AddNodes(DependencyInfo targetVertex, DependencyInfo sourceVertex)
        {
            var sourceKey = Tuple.Create(sourceVertex.ParentInstance, sourceVertex.PropertyInfo.Name);
            var targetKey = Tuple.Create(targetVertex.ParentInstance, targetVertex.PropertyInfo.Name);
            if (!nodeLookup.ContainsKey(sourceKey))
                nodeLookup.Add(sourceKey, sourceVertex);
            else
                nodeLookup[sourceKey].Merge(sourceVertex);

            if (!nodeLookup.ContainsKey(targetKey))
                nodeLookup.Add(targetKey, targetVertex);
            else
                nodeLookup[targetKey].Merge(targetVertex);

            graph.AddEdge(nodeLookup[sourceKey], nodeLookup[targetKey]);
        }

        private void SwitchInstanceWhenChanged(DependencyInfo sourceVertex)
        {
            // TODO I think we also need to track here
            instancesToSwitch.Add(sourceVertex, sourceVertex.GetValue());
        }

        private void TrackLeafDependency(DependencyInfo sourceVertex)
        {
            var value = sourceVertex.GetValue();
            if (!leafDependencies.ContainsKey(sourceVertex))
                leafDependencies.Add(sourceVertex, value);
            var sourceKey = Tuple.Create<object, string>(value, null);
            if (!nodeLookup.ContainsKey(sourceKey))
                nodeLookup.Add(sourceKey, sourceVertex);

            TrackInstanceIfNeeded(value);
        }

        private void TrackInstanceIfNeeded(object instance)
        {
            if (!instancesBeingTracked.Contains(instance))
            {
                instancesBeingTracked.Add(instance);
                foreach (var notificationStrategy in notificationStrategies)
                {
                    notificationStrategy.Track(instance);
                }
            }
        }

        private void ForgetInstance(object instance)
        {
            if (!instancesBeingTracked.Contains(instance))
            {
                instancesBeingTracked.Add(instance);
                foreach (var notificationStrategy in notificationStrategies)
                {
                    notificationStrategy.Untrack(instance);
                }
            }
        }
    }
}