using System;
using ReactGraph.Notification;

namespace ReactGraph.NodeInfo
{
    class WritableNodeInfo<T> : IWritableNodeInfo<T>
    {
        readonly NodeRepository nodeRepository;
        readonly string label;
        readonly string key;
        readonly INotificationStrategy[] notificationStrategies;
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly Func<T> getValue;
        readonly Action<T> setValue;
        IValueSource<T> formula;
        object parentInstance;
        Action<Exception> exceptionHandler;

        public WritableNodeInfo(
            object parentInstance,
            INotificationStrategy[] notificationStrategies,
            Func<T> getValue,
            Action<T> setValue, 
            NodeRepository nodeRepository,
            string label,
            string key)
        {
            this.notificationStrategies = notificationStrategies;
            this.setValue = setValue;
            this.nodeRepository = nodeRepository;
            this.label = label;
            this.key = key;
            this.getValue = getValue;
            this.parentInstance = parentInstance;
            ValueChanged();
        }

        public void SetSource(IValueSource<T> formulaNode, Action<Exception> errorHandler)
        {
            if (formula != null)
                throw new InvalidOperationException(string.Format("{0} already has a formula associated with it", label));

            formula = formulaNode;
            exceptionHandler = errorHandler;
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

        public override string ToString()
        {
            return label;
        }

        IMaybe IValueSource.GetValue()
        {
            return GetValue();
        }

        public ReevalResult Reevaluate()
        {
            if (formula != null)
            {
                ValueChanged();
                var value = formula.GetValue();
                if (value.HasValue)
                {
                    // TODO Don't set and return NoChange when value has not changed
                    setValue(value.Value);
                    return ReevalResult.Changed;
                }

                exceptionHandler(value.Exception);
                return ReevalResult.Error;
            }

            return ReevalResult.NoChange;
        }

        public void ValueChanged()
        {
            if (currentValue.HasValue)
            {
                foreach (var notificationStrategy in notificationStrategies)
                    notificationStrategy.Untrack(currentValue.Value);
                nodeRepository.RemoveLookup(currentValue.Value, null);
            }

            try
            {
                currentValue.NewValue(getValue());
            }
            catch (Exception ex)
            {
                currentValue.CouldNotCalculate(ex);
            }

            if (currentValue.HasValue)
            {
                nodeRepository.AddLookup(currentValue.Value, null, this);
                foreach (var notificationStrategy in notificationStrategies)
                    notificationStrategy.Track(currentValue.Value);
            }
        }

        public void UpdateSubscriptions(IMaybe newParent)
        {
            nodeRepository.RemoveLookup(parentInstance, key);
            if (newParent.HasValue)
            {
                parentInstance = newParent.Value;
                nodeRepository.AddLookup(parentInstance, key, this);
            }
        }
    }

    enum ReevalResult
    {
        NoChange,
        Error,
        Changed
    }
}