using ReactGraph.NodeInfo;

namespace ReactGraph.Instrumentation
{
    public interface IEngineInstrumentation
    {
        void OnDependencyWalkStart(long walkIndex, string sourceProperty, string nodeId);
        void OnNodeEvaluated(long walkIndex, string updatedNode, string nodeId, ReevaluationResult result);
        void OnDependendencyWalkEnd(long walkIndex);
    }
}