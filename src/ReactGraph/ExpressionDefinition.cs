using System.Linq.Expressions;
using ReactGraph.Construction;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class ExpressionDefinition<T> : IDefinitionIdentity
    {
        public ExpressionDefinition(Expression expression, NodeType nodeType, string nodeName)
        {
            NodeType = nodeType;
            NodeName = nodeName;
            Path = ExpressionStringBuilder.ToString(expression);
        }

        public string Path { get; private set; }
        public string NodeName { get; private set; }
        public NodeType NodeType { get; private set; }

        public override string ToString()
        {
            return Path;
        }
    }
}