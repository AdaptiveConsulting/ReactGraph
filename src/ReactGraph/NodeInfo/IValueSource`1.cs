namespace ReactGraph.NodeInfo
{
    interface IValueSource<T> : INodeInfo
    {
        new Maybe<T> GetValue();
    }
}