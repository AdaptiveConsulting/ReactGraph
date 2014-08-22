using System.Linq.Expressions;
using ReactGraph.Construction;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class ExpressionDefinition<T> : IDefinitionIdentity
    {
        public ExpressionDefinition(Expression expression, NodeType nodeType, string nodeName, object root)
        {
            NodeType = nodeType;
            NodeName = nodeName;
            Path = ExpressionStringBuilder.ToString(expression);
            Root = root;
        }

        public object Root { get; private set; }
        public string Path { get; private set; }
        public string NodeName { get; private set; }
        public NodeType NodeType { get; private set; }

        public override string ToString()
        {
            return Path;
        }
    }
}