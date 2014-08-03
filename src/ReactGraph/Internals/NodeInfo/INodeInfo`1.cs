namespace ReactGraph.Internals.NodeInfo
{
    interface INodeInfo<out T> : INodeInfo, IValueSource<T>
    {
    }
}