using ReactGraph.NodeInfo;

namespace ReactGraph.Instrumentation
{
    public interface IEngineInstrumentation
    {
        void OnDependencyWalkStart(long walkIndex, string sourceProperty);
        void OnNodeEvaluated(long walkIndex, string updatedNode, ReevaluationResult result);
        void OnDepdendencyWalkEnd(long walkIndex);
    }
}