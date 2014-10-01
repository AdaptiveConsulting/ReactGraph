namespace ReactGraph.NodeInfo
{
    class NodeMetadata : INodeMetadata
    {
        public VisualisationInfo VisualisationInfo { get; private set; }

        public string Label { get; private set; }
        public string Id { get; private set; }

        public NodeMetadata(VisualisationInfo visualisationInfo, string label, string id)
        {
            this.VisualisationInfo = visualisationInfo;
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
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Label;
        }
    }
}