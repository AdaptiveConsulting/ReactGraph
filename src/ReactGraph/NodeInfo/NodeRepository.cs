using System;
using System.Collections.Generic;

namespace ReactGraph.NodeInfo
{
    class NodeRepository
    {
        private readonly Dictionary<Tuple<object, string>, INodeInfo> nodeLookup;

        public NodeRepository()
        {
            nodeLookup = new Dictionary<Tuple<object, string>, INodeInfo>();
        }

        public bool Contains(object instance, string key)
        {
            return nodeLookup.ContainsKey(Tuple.Create(instance, key));
        }

        public INodeInfo Get(object instance, string key)
        {
            return nodeLookup[Tuple.Create(instance, key)];
        }

        public void RemoveLookup(object instance, string key)
        {
            var tuple = Tuple.Create(instance, key);
            if (nodeLookup.ContainsKey(tuple))
                nodeLookup.Remove(tuple);
        }

        public void AddLookup(object instance, string key, INodeInfo nodeInfo)
        {
            var tuple = Tuple.Create(instance, key);
            if (!nodeLookup.ContainsKey(tuple))
                nodeLookup.Add(tuple, nodeInfo);
        }
    }
}