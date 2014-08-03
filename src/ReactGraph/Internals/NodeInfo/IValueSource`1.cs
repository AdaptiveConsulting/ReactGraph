namespace ReactGraph.Internals.NodeInfo
{
    interface IValueSource<T> : IValueSource
    {
        new Maybe<T> GetValue();
    }
}