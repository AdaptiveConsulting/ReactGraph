using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public class DependencyEngine
    {
        private readonly DirectedGraph<NodeInfo> _graph;
        private readonly ExpressionParser _expressionParser;
        private readonly ReactEngine _reactEngine;

        public DependencyEngine()
        {
            _graph = new DirectedGraph<NodeInfo>();
            _expressionParser = new ExpressionParser();
            _reactEngine = new ReactEngine(Graph);
        }

        public ReactEngine ReactEngine
        {
            get { return _reactEngine; }
        }

        public DirectedGraph<NodeInfo> Graph
        {
            get { return _graph; }
        }

        public void Bind<TProp>(Expression<Func<TProp>> targetProperty, Expression<Func<TProp>> sourceFunction)
        {
            _expressionParser.AddToGraph(Graph, targetProperty, sourceFunction);
        }
    }
}