namespace ReactGraph.Internals.NodeInfo
{
    interface IValueSink<in T>
    {
        void SetSource(IValueSource<T> formulaNode);
    }
}