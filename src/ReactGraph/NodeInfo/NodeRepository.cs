using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ReactGraph.NodeInfo
{
    class NodeRepository
    {
        // HACK this is a very quick hack to get INotifyPropertyChanged support
        readonly DependencyEngine engine;
        private readonly Dictionary<object, INodeInfo> nodeLookup;

        public NodeRepository(DependencyEngine engine)
        {
            this.engine = engine;
            nodeLookup = new Dictionary<object, INodeInfo>();
        }

        public bool Contains(object instance)
        {
            return nodeLookup.ContainsKey(instance);
        }

        public INodeInfo Get(object instance)
        {
            return nodeLookup[instance];
        }

        public void RemoveLookup(object instance)
        {
            if (instance != null && nodeLookup.ContainsKey(instance))
            {
                var notifyPropertyChanged = instance as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                    notifyPropertyChanged.PropertyChanged -= NotifyPropertyChangedOnPropertyChanged;
                nodeLookup.Remove(instance);
            }
        }

        void NotifyPropertyChangedOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            engine.ValueHasChanged(sender, propertyChangedEventArgs.PropertyName);
        }

        public void AddLookup(object instance, INodeInfo nodeInfo)
        {
            if (instance != null && !nodeLookup.ContainsKey(instance))
            {
                var notifyPropertyChanged = instance as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                    notifyPropertyChanged.PropertyChanged += NotifyPropertyChangedOnPropertyChanged;
                nodeLookup.Add(instance, nodeInfo);
            }
        }
    }
}