namespace ReactGraph.NodeInfo
{
    interface INodeInfo
    {
        NodeType Type { get; }

        string FullPath { get; }

        ReevaluationResult Reevaluate();

        void ValueChanged();

        bool PathMatches(string pathToChangedValue);
    }
}