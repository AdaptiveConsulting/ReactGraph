namespace ReactGraph.NodeInfo
{
    class NodeMetadata : INodeMetadata
    {
        public NodeType NodeType { get; private set; }
        public string Label { get; private set; }
        public string Id { get; private set; }

        public NodeMetadata(NodeType nodeType, string label, string id)
        {
            NodeType = nodeType;
            Label = label;
            Id = id;
        }
    }
}