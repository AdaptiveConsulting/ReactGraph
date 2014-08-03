namespace ReactGraph.Internals.NodeInfo
{
    interface IWritableNodeInfo<T> : INodeInfo<T>, IValueSink<T>
    {
    }
}