using ReactGraph.Graph;
using ReactGraph.NodeInfo;

namespace ReactGraph.Api
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