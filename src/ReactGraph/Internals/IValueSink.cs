namespace ReactGraph.Internals
{
    interface IValueSink<in T>
    {
        void SetSource(IValueSource<T> formulaNode);
    }
}