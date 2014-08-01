namespace ReactGraph.Internals
{
    internal interface INodeInfo
    {
        void Reevaluate();
        void ValueChanged();
        object ParentInstance { get; set; }
        string Key { get; set; }
    }
}