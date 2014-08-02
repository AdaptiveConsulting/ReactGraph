using System;
using System.Linq.Expressions;
using ReactGraph.Internals.Graph;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Construction
{
    internal class ExpressionDefinition : IExpressionDefinition
    {
        readonly INodeInfo expressionNode;
        readonly ExpressionParser expressionParser;
        readonly DirectedGraph<INodeInfo> graph;
        string label;
        string color;

        public ExpressionDefinition(INodeInfo expressionNode, ExpressionParser expressionParser, DirectedGraph<INodeInfo> graph)
        {
            this.expressionNode = expressionNode;
            this.expressionParser = expressionParser;
            this.graph = graph;
        }

        public IExpressionDefinition Metadata(string lbl = null, string clr = null)
        {
            label = lbl;
            color = clr;
            return this;
        }

        public IMemberDefinition Bind<TProp>(Expression<Func<TProp>> targetProperty)
        {
            var targetPropertyNode = expressionParser.GetNodeInfo(targetProperty);
            var valueSink = targetPropertyNode as IValueSink<TProp>;

            if (valueSink == null)
                throw new Exception("Target expression cannot be written to");

            // TODO We probably need another interface or a base type here to remove cast
            valueSink.SetSource((IValueSource<TProp>)expressionNode);

            var edge = graph.AddEdge(expressionNode, targetPropertyNode);
            edge.Source.Color = color;
            edge.Source.Label = label;
            AddDependenciesToGraph(expressionNode);
            return new MemberDefinition(targetPropertyNode, edge.Target);
        }

        private void AddDependenciesToGraph(INodeInfo formulaNode)
        {
            foreach (var dependency in formulaNode.Dependencies)
            {
                graph.AddEdge(dependency, formulaNode);
                AddDependenciesToGraph(dependency);
            }
        }
    }
}