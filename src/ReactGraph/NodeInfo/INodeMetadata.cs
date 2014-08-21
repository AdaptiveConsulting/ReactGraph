namespace ReactGraph.NodeInfo
{
    public interface INodeMetadata
    {
        NodeType NodeType { get; }
        string Label { get; }
        string Id { get; }
    }
}