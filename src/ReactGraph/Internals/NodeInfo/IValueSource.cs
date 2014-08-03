namespace ReactGraph.Internals.NodeInfo
{
    interface IValueSource
    {
        object GetValue();
    }

    interface IValueSource<out T> : IValueSource
    {
        new T GetValue();
    }
}