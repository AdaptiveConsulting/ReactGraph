using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public interface IDefinitionIdentity
    {
        string FullPath { get; }

        string NodeName { get; }

        NodeType NodeType { get; }
    }
}