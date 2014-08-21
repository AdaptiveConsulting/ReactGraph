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

        protected bool Equals(NodeMetadata other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NodeMetadata) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}