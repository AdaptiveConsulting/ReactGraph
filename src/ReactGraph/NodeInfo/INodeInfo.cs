namespace ReactGraph.NodeInfo
{
    interface INodeInfo : IValueSource
    {
        ReevalResult Reevaluate();

        void ValueChanged();

        void UpdateSubscriptions(IMaybe newParent);
    }
}