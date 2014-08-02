using System.ComponentModel;

namespace ReactGraph
{
    class NotifyPropertyChangedStrategy : INotificationStrategy
    {
        private readonly DependencyEngine dependencyEngine;

        public NotifyPropertyChangedStrategy(DependencyEngine dependencyEngine)
        {
            this.dependencyEngine = dependencyEngine;
        }

        public void Track(object instance)
        {
            var notifyPropertyChanged = instance as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += NotifyPropertyChangedOnPropertyChanged;
            }
        }

        public void Untrack(object instance)
        {
            var notifyPropertyChanged = instance as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= NotifyPropertyChangedOnPropertyChanged;
            }
        }

        public bool AppliesTo(object instance)
        {
            return instance is INotifyPropertyChanged;
        }

        private void NotifyPropertyChangedOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            dependencyEngine.ValueHasChanged(sender, propertyChangedEventArgs.PropertyName);
            dependencyEngine.ValueHasChanged(sender, null);
        }
    }
}