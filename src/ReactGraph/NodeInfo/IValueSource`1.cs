namespace ReactGraph.NodeInfo
{
    interface IValueSource<T> : IValueSource
    {
        Maybe<T> GetValue();
        void TrackChanges();
        void SetTarget(ITakeValue<T> targetNode, Maybe<T> initialValue);
    }
}