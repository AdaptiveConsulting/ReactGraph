using ReactGraph.NodeInfo;

namespace ReactGraph.Construction
{
    abstract class DependencyDescriptor<T> : DependencyDescriptor
    {
        public abstract IWritableNodeInfo<T> GetOrCreateWritableNodeInfo(NodeRepository repo);
    }
}