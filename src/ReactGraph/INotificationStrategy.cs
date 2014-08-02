namespace ReactGraph
{
    public interface INotificationStrategy
    {
        void Track(object instance);
        void Untrack(object instance);
    }
}