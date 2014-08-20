using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ReactGraph.Notification
{
    class NotifyPropertyChangedStrategy : INotificationStrategy
    {
        readonly List<INotifyPropertyChanged> tracking = new List<INotifyPropertyChanged>();
        readonly DependencyEngine dependencyEngine;

        public NotifyPropertyChangedStrategy(DependencyEngine dependencyEngine)
        {
            this.dependencyEngine = dependencyEngine;
        }

        public void Track(object instance)
        {
            var notifyPropertyChanged = instance as INotifyPropertyChanged;
            if (notifyPropertyChanged != null && !tracking.Contains(notifyPropertyChanged))
            {
                tracking.Add(notifyPropertyChanged);
                notifyPropertyChanged.PropertyChanged += NotifyPropertyChangedOnPropertyChanged;
            }
        }

        public void Untrack(object instance)
        {
            var notifyPropertyChanged = instance as INotifyPropertyChanged;
            if (notifyPropertyChanged != null && tracking.Contains(notifyPropertyChanged))
            {
                tracking.Remove(notifyPropertyChanged);
                notifyPropertyChanged.PropertyChanged -= NotifyPropertyChangedOnPropertyChanged;
            }
        }

        public bool AppliesTo(Type type)
        {
            return typeof(INotifyPropertyChanged).IsAssignableFrom(type);
        }

        private void NotifyPropertyChangedOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!dependencyEngine.ValueHasChanged(sender, propertyChangedEventArgs.PropertyName))
                dependencyEngine.ValueHasChanged(sender, null);
        }
    }
}