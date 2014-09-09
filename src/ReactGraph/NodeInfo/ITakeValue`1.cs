namespace ReactGraph.NodeInfo
{
    interface ITakeValue<T> : INodeInfo
    {
        void SetSource(IValueSource<T> sourceNode);
    }
}