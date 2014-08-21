using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public interface IDefinitionIdentity
    {
        object Root { get; }

        string Path { get; }

        string NodeName { get; }

        NodeType NodeType { get; }
    }
}