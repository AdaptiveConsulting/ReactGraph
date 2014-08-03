using System;
using ReactGraph.Internals.Construction;
using ReactGraph.Internals.Notification;

namespace ReactGraph.Internals.NodeInfo
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

        public void SetSource(IValueSource<T> formulaNode)
        {
            formula = formulaNode;
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

        public void Reevaluate()
        {
            if (formula != null)
            {
                ValueChanged();
                var value = formula.GetValue();
                if (value.HasValue)
                    setValue(value.Value);
            }
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
            catch (FormulaNullReferenceException)
            {
                currentValue.ValueMissing();
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
}