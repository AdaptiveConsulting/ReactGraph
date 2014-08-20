using System.Collections.Generic;
using ReactGraph.NodeInfo;

namespace ReactGraph.Construction
{
    abstract class DependencyDescriptor
    {
        protected DependencyDescriptor()
        {
            Dependencies = new List<DependencyDescriptor>();
        }

        public abstract object GetValue();

        public abstract INodeInfo GetOrCreateNodeInfo(NodeRepository repo);

        public object RootInstance { get; set; }

        public object ParentInstance { get; protected set; }

        public List<DependencyDescriptor> Dependencies { get; private set; }
    }
}
