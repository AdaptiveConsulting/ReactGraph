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

        public IMemberDefinition Bind(Expression<Func<T>> targetProperty)
        {
            if (edge != null)
                throw new InvalidOperationException("You can only bind a single expression to a property");

            var targetPropertyDescriptor = expressionParser.GetTargetInfo(targetProperty);
            var targetNode = targetPropertyDescriptor.GetOrCreateWritableNodeInfo(repo);

            targetNode.SetSource(formulaNode);

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