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

        public void Bind<TProp>(Expression<Func<TProp>> targetProperty, Expression<Func<TProp>> sourceFunction)
        {
            var targetVertex = expressionParser.GetNodeInfo(targetProperty, sourceFunction);
            TrackInstanceIfNeeded(targetVertex.RootInstance);
            var sourceVertices = expressionParser.GetSourceVerticies(sourceFunction);

            ProcessNodes(sourceVertices, targetVertex, targetVertex);
        }

        public override string ToString()
        {
            return graph.ToDotLanguage("Dependency Graph");
        }

        private void ProcessNodes(DependencyInfo[] sourceVertices, DependencyInfo targetVertex, DependencyInfo originalTarget)
        {
            foreach (var sourceVertex in sourceVertices)
            {
                TrackInstanceIfNeeded(sourceVertex.RootInstance);
                AddNodes(targetVertex, sourceVertex);
                if (targetVertex != originalTarget)
                {
                    AddNodes(originalTarget, sourceVertex);
                    SwitchInstanceWhenChanged(sourceVertex);
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
            instancesToSwitch.Add(sourceVertex, sourceVertex.GetValue());
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