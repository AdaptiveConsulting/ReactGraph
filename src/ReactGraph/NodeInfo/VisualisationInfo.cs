namespace ReactGraph.NodeInfo
{
    public class VisualisationInfo
    {
        readonly bool isRoot;

        public VisualisationInfo(NodeType nodeType, bool isRoot)
        {
            this.isRoot = isRoot;
            NodeType = nodeType;
        }

        public bool IsRoot { get { return isRoot && !IsDirectlyReferenced; } }

        public bool IsDirectlyReferenced { get; set; }

        public NodeType NodeType { get; set; }
    }
}