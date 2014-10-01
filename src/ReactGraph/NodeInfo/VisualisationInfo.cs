namespace ReactGraph.NodeInfo
{
    public class VisualisationInfo
    {
        public VisualisationInfo(NodeType nodeType)
        {
            this.NodeType = nodeType;
        }

        public bool IsRoot { get; set; }

        public bool IsDirectlyReferenced { get; set; }

        public NodeType NodeType { get; set; }
    }
}