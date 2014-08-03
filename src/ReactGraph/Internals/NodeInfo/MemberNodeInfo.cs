using System;
using ReactGraph.Internals.Notification;

namespace ReactGraph.Internals.NodeInfo
{
    class MemberNodeInfo<T> : IWritableNodeInfo<T>
    {
        readonly NodeRepository nodeRepository;
        readonly string label;
        readonly string key;
        readonly INotificationStrategy[] notificationStrategies;
        readonly Func<T> getValue;
        readonly Action<T> setValue;
        IValueSource<T> formula;
        T currentValue;
        object parentInstance;

        public MemberNodeInfo(
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

        public T GetValue()
        {
            return currentValue;
        }

        public override string ToString()
        {
            return label;
        }

        object IValueSource.GetValue()
        {
            return GetValue();
        }

        public void Reevaluate()
        {
            if (formula != null)
            {
                ValueChanged();
                setValue(formula.GetValue());
            }
        }

        public void ValueChanged()
        {
            foreach (var notificationStrategy in notificationStrategies)
                notificationStrategy.Untrack(currentValue);
            nodeRepository.RemoveLookup(currentValue, null);

            currentValue = getValue();

            nodeRepository.AddLookup(currentValue, null, this);
            foreach (var notificationStrategy in notificationStrategies)
                notificationStrategy.Track(currentValue);
        }

        public void UpdateSubscriptions(object newParent)
        {
            nodeRepository.RemoveLookup(parentInstance, key);
            parentInstance = newParent;
            nodeRepository.AddLookup(parentInstance, key, this);
        }
    }
}