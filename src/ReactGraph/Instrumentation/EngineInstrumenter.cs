using ReactGraph.NodeInfo;
using ReactGraph.Properties;

namespace ReactGraph.Instrumentation
{
    class EngineInstrumenter
    {
        readonly IEngineInstrumentation engineInstrumentation;
        long walkIndex;

        public EngineInstrumenter([CanBeNull] IEngineInstrumentation engineInstrumentation)
        {
            this.engineInstrumentation = engineInstrumentation;
        }

        public void DependecyWalkStarted(string sourceProperty)
        {
            if (engineInstrumentation == null) return;
            walkIndex++;

            engineInstrumentation.OnDependencyWalkStart(walkIndex, sourceProperty);
        }

        public void NodeEvaluated(string updatedNode, ReevaluationResult result)
        {
            if (engineInstrumentation != null) engineInstrumentation.OnNodeEvaluated(walkIndex, updatedNode, result);
        }

        public void DependencyWalkEnded()
        {
            if (engineInstrumentation != null) engineInstrumentation.OnDepdendencyWalkEnd(walkIndex);
        }
    }
}