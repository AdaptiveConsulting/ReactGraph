namespace ReactGraph.Internals.NodeInfo
{
    interface IValueSink<T>
    {
        void SetSource(IValueSource<T> formulaNode);
    }
}