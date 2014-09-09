namespace ReactGraph.NodeInfo
{
    interface IValueSource : INodeInfo
    {
        void UnderlyingValueHasBeenChanged();
    }
}
