namespace ReactGraph.NodeInfo
{
    interface IValueSource : INodeInfo
    {
        IMaybe GetValue();
    }
}
