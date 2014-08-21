namespace ReactGraph.NodeInfo
{
    interface INodeInfo : IValueSource
    {
        NodeType Type { get; }

        ReevaluationResult Reevaluate();

        void ValueChanged();
    }
}