namespace ReactGraph.NodeInfo
{
    public interface INodeMetadata
    {
        VisualisationInfo VisualisationInfo { get; }
        string Label { get; }
        string Id { get; }
    }
}