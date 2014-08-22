using System.Collections.Generic;

namespace ReactGraph.NodeInfo
{
    class NodeRepository
    {
        private readonly Dictionary<object, INodeInfo> nodeLookup;

        public NodeRepository()
        {
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
            if (nodeLookup.ContainsKey(instance))
                nodeLookup.Remove(instance);
        }

        public void AddLookup(object instance, INodeInfo nodeInfo)
        {
            if (instance != null && !nodeLookup.ContainsKey(instance))
                nodeLookup.Add(instance, nodeInfo);
        }
    }
}