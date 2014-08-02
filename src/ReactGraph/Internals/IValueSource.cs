namespace ReactGraph.Internals
{
    interface IValueSource<out T>
    {
        T GetValue();
    }
}