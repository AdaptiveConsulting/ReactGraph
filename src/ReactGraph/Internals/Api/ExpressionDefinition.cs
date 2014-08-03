using System;
using System.Linq.Expressions;
using ReactGraph.Internals.Construction;
using ReactGraph.Internals.Graph;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Api
{
    internal class ExpressionDefinition<T> : IExpressionDefinition<T>
    {
        readonly ExpressionParser expressionParser;
        readonly DirectedGraph<INodeInfo> graph;
        readonly NodeRepository repo;
        readonly INodeInfo<T> formulaNode;
        Edge<INodeInfo> edge;
        string label;
        string color;

        public ExpressionDefinition(
            FormulaDescriptor<T> formulaDescriptor, ExpressionParser expressionParser, 
            DirectedGraph<INodeInfo> graph, NodeRepository repo)
        {
            this.expressionParser = expressionParser;
            this.graph = graph;
            this.repo = repo;
            formulaNode = (INodeInfo<T>) formulaDescriptor.GetOrCreateNodeInfo(repo);
            AddDependenciesToGraph(formulaDescriptor);
        }

        public IExpressionDefinition<T> Metadata(string lbl = null, string clr = null)
        {
            if (edge == null)
            {
                label = lbl;
                color = clr;
            }
            else
            {
                edge.Source.Color = color;
                edge.Source.Label = label;
            }
            return this;
        }

        public IMemberDefinition Bind<TProp>(Expression<Func<TProp>> targetProperty, Action<Exception> onError)
        {
            if (edge != null)
                throw new InvalidOperationException("You can only bind a single expression to a property");

            var targetPropertyDescriptor = expressionParser.GetTargetInfo(targetProperty);
            var targetNode = targetPropertyDescriptor.GetOrCreateWritableNodeInfo(repo);

            var valueSource = formulaNode as IValueSource<TProp>;
            if (valueSource == null)
            {
                var message = string.Format("Cannot bind target of type {0} to source of type {1}", typeof(TProp), typeof(T));
                throw new ArgumentException(message);
            }

            targetNode.SetSource(valueSource);

            edge = graph.AddEdge(formulaNode, targetNode);
            edge.Source.Color = color;
            edge.Source.Label = label;
            return new MemberDefinition(edge.Target);
        }

        private void AddDependenciesToGraph(DependencyDescriptor descriptor)
        {
            foreach (var dependency in descriptor.Dependencies)
            {
                graph.AddEdge(dependency.GetOrCreateNodeInfo(repo), formulaNode);
                AddDependenciesToGraph(dependency);
            }
        }
    }
}