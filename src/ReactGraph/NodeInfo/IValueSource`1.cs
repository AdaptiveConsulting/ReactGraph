namespace ReactGraph.NodeInfo
{
    interface IValueSource<T> : IValueSource
    {
        new Maybe<T> GetValue();
        void TrackChanges();
        void SetTarget(ITakeValue<T> targetNode);
    }
}