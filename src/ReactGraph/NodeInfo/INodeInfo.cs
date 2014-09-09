namespace ReactGraph.NodeInfo
{
    interface INodeInfo
    {
        NodeType VisualisationNodeType { get; }

        string Path { get; }

        ReevaluationResult Reevaluate();

        void ValueChanged();
    }
}