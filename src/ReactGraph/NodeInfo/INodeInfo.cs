namespace ReactGraph.NodeInfo
{
    interface INodeInfo : IValueSource
    {
        ReevaluationResult Reevaluate();

        void ValueChanged();

        void UpdateSubscriptions(IMaybe newParent);
    }
}