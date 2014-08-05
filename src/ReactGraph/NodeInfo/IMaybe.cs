namespace ReactGraph.NodeInfo
{
    interface IMaybe
    {
        object Value { get; }

        bool HasValue { get; }
    }
}