using System.Collections.Generic;

namespace ReactGraph.Internals
{
    public class Notifications
    {
        private readonly List<object> instancesBeingTracked = new List<object>();
        private readonly List<INotificationStrategy> notificationStrategies;

        public Notifications(DependencyEngine engine)
        {
            notificationStrategies = new List<INotificationStrategy>
            {
                new NotifyPropertyChangedStrategy(engine)
            };
        }

        public void TrackInstanceIfNeeded(object instance)
        {
            //TODO Check if reference type
            if (!instancesBeingTracked.Contains(instance))
            {
                instancesBeingTracked.Add(instance);
                foreach (var notificationStrategy in notificationStrategies)
                {
                    notificationStrategy.Track(instance);
                }
            }
        }

        public void ForgetInstance(object instance)
        {
            if (instancesBeingTracked.Contains(instance))
            {
                instancesBeingTracked.Remove(instance);
                foreach (var notificationStrategy in notificationStrategies)
                {
                    notificationStrategy.Untrack(instance);
                }
            }
        }
    }
}