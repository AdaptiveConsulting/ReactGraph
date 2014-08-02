using System;
using System.Linq.Expressions;

namespace ReactGraph.Internals
{
    internal class ExpressionDefinition : IExpressionDefinition
    {
        readonly INodeInfo expressionNode;
        readonly ExpressionParser expressionParser;
        readonly DirectedGraph<INodeInfo> graph;

        public ExpressionDefinition(INodeInfo expressionNode, ExpressionParser expressionParser, DirectedGraph<INodeInfo> graph)
        {
            this.expressionNode = expressionNode;
            this.expressionParser = expressionParser;
            this.graph = graph;
        }

        public void Bind<TProp>(Expression<Func<TProp>> targetProperty)
        {
            var targetVertex = expressionParser.GetNodeInfo(targetProperty);
            var valueSink = targetVertex as IValueSink<TProp>;

            if (valueSink == null)
                throw new Exception("Target expression cannot be written to");

            // TODO We probably need another interface or a base type here to remove cast
            valueSink.SetSource((IValueSource<TProp>)expressionNode);

            graph.AddEdge(expressionNode, targetVertex);
            AddDependenciesToGraph(expressionNode);
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