namespace ReactGraph.Internals.NodeInfo
{
    interface IMaybe
    {
        object Value { get; }

        bool HasValue { get; }
    }
}