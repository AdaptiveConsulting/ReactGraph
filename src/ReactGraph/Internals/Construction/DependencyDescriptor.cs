using System.Collections.Generic;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Construction
{
    abstract class DependencyDescriptor
    {
        protected DependencyDescriptor()
        {
            Dependencies = new List<DependencyDescriptor>();
        }

        public abstract object GetValue();

        public abstract INodeInfo GetOrCreateNodeInfo(NodeRepository repo);

        public abstract bool IsReadOnly { get; }

        public object RootInstance { get; set; }

        public object ParentInstance { get; set; }

        public List<DependencyDescriptor> Dependencies { get; private set; }
    }
}
