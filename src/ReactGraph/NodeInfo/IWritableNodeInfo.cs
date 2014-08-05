namespace ReactGraph.NodeInfo
{
    interface IWritableNodeInfo<T> : INodeInfo<T>, IValueSink<T>
    {
    }
}