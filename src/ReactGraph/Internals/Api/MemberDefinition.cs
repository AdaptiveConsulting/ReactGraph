using ReactGraph.Internals.Graph;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Api
{
    class MemberDefinition : IMemberDefinition
    {
        readonly Vertex<INodeInfo> propertyVertex;

        public MemberDefinition(Vertex<INodeInfo> propertyVertex)
        {
            this.propertyVertex = propertyVertex;
        }
    }
}