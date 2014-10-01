namespace ReactGraph.NodeInfo
{
    interface INodeInfo
    {
        VisualisationInfo VisualisationInfo { get; }

        string FullPath { get; }

        ReevaluationResult Reevaluate();

        bool PathMatches(string pathToChangedValue);
    }
}