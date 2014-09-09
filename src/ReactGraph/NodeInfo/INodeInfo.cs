namespace ReactGraph.NodeInfo
{
    interface INodeInfo
    {
        NodeType VisualisationNodeType { get; }

        string FullPath { get; }

        ReevaluationResult Reevaluate();

        bool PathMatches(string pathToChangedValue);
    }
}