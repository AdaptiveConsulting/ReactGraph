namespace ReactGraph.NodeInfo
{
    interface INodeInfo
    {
        NodeType Type { get; }

        ReevaluationResult Reevaluate();

        void ValueChanged();
    }
}