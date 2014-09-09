namespace ReactGraph.NodeInfo
{
    interface INodeInfo
    {
        NodeType VisualisationNodeType { get; }

        string FullPath { get; }

        ReevaluationResult Reevaluate();

        void ValueChanged();

        bool PathMatches(string pathToChangedValue);
    }
}