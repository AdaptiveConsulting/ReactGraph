namespace ReactGraph.NodeInfo
{
    interface INodeInfo
    {
        NodeType Type { get; }

        string Path { get; }

        ReevaluationResult Reevaluate();
    }
}