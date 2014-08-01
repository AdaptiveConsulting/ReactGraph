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
        private readonly Dictionary<Tuple<object, string>, INodeInfo> nodeLookup = new Dictionary<Tuple<object, string>, INodeInfo>();
        private readonly DirectedGraph<INodeInfo> graph;
        private readonly ExpressionParser expressionParser;
        private bool isExecuting;

        public DependencyEngine()
        {
            graph = new DirectedGraph<INodeInfo>();
            expressionParser = new ExpressionParser();
        }

        public event Action<object, string> SettingValue = (o, s) => { };

        public void PropertyChanged(object instance, string property)
        {
            var key = Tuple.Create(instance, property);
            if (!nodeLookup.ContainsKey(key)) return;
            var node = nodeLookup[key];
            
            if (isExecuting) return;
            try
            {
                isExecuting = true;
                var orderToReeval = graph.TopologicalSort(node).ToArray();
                orderToReeval.First().Data.ValueChanged();
                foreach (var vertex in orderToReeval.Skip(1))
                {
                    //SettingValue(vertex.Data.RootInstance, vertex.Data.PropertyInfo.Name);
                    vertex.Data.Reevaluate();
                }
            }
            finally
            {
                isExecuting = false;
            }
        }

        internal void ValueChanged(object instance, object newInstance)
        {
            
        }

        private void UpdateLeafDependencies(PropertyNodeInfo<> node)
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

        private void SwitchTransientInstances(PropertyNodeInfo<> node)
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
            var targetVertex = expressionParser.GetNodeInfo(targetProperty);
            TrackInstanceIfNeeded(targetVertex.RootInstance);
            var formulaNode = expressionParser.GetFormulaExpressionInfo(sourceFunction);

            targetVertex.SetPropertySource(formulaNode);

            ProcessNodes(formulaNode.Dependencies, targetVertex, targetVertex);
        }

        public override string ToString()
        {
            return graph.ToDotLanguage("DependencyGraph");
        }

        private void ProcessNodes<T>(INodeInfo[] sourceVertices, PropertyNodeInfo<T> targetVertex, PropertyNodeInfo<T> originalTarget)
        {
            foreach (var sourceVertex in sourceVertices)
            {
                AddNodes(targetVertex, sourceVertex);
                // When different the target vertex is a transient property, i.e Foo in viewModel.Foo.Bar 
                if (targetVertex != originalTarget)
                {
                    AddNodes(originalTarget, sourceVertex);
                }

                var propertyInfo = sourceVertex as PropertyNodeInfo<T>;
                if (propertyInfo != null)
                {
                    if (propertyInfo.PropertyExpression != null)
                    {
                        var parent = propertyInfo.PropertyExpression.Expression;
                        if (parent is MemberExpression)
                        {
                            var subNodes = expressionParser.GetFormulaExpressionInfo(parent);

                            ProcessNodes(subNodes.Dependencies, sourceVertex, originalTarget);
                        }
                    }
                }
            }
        }

        private void AddNodes(INodeInfo targetVertex, INodeInfo sourceVertex)
        {
            var sourceKey = Tuple.Create(sourceVertex.ParentInstance, sourceVertex.Key);
            var targetKey = Tuple.Create(targetVertex.ParentInstance, targetVertex.Key);
            if (!nodeLookup.ContainsKey(sourceKey))
                nodeLookup.Add(sourceKey, sourceVertex);

            if (!nodeLookup.ContainsKey(targetKey))
                nodeLookup.Add(targetKey, targetVertex);

            graph.AddEdge(nodeLookup[sourceKey], nodeLookup[targetKey]);
        }
    }
}