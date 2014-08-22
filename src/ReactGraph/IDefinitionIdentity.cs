using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public interface IDefinitionIdentity
    {
        string Path { get; }

        string NodeName { get; }

        NodeType NodeType { get; }
    }
}