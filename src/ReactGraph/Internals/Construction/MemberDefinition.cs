using ReactGraph.Internals.Graph;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Construction
{
    class MemberDefinition : IMemberDefinition
    {
        readonly Vertex<INodeInfo> propertyVertex;

        public MemberDefinition(Vertex<INodeInfo> propertyVertex)
        {
            this.propertyVertex = propertyVertex;
        }

        public IMemberDefinition Metadata(string label = null, string color = null)
        {
            propertyVertex.Color = color;
            propertyVertex.Label = label;
            return this;
        }
    }
}