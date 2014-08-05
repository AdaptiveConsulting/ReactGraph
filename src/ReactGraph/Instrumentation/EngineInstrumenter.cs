using System;
using ReactGraph.NodeInfo;

namespace ReactGraph.Instrumentation
{
    class EngineInstrumenter
    {
        readonly IEngineInstrumentation engineInstrumentation;
        long walkIndex;

        public EngineInstrumenter(IEngineInstrumentation engineInstrumentation)
        {
            if(engineInstrumentation == null) throw new ArgumentNullException("engineInstrumentation");

            this.engineInstrumentation = engineInstrumentation;
        }

        public void DependecyWalkStarted(string sourceProperty)
        {
            walkIndex++;

            engineInstrumentation.OnDependencyWalkStart(walkIndex, sourceProperty);
        }

        public void NodeEvaluated(string updatedNode, ReevaluationResult result)
        {
            engineInstrumentation.OnNodeEvaluated(walkIndex, updatedNode, result);
        }

        public void DependencyWalkEnded()
        {
            engineInstrumentation.OnDepdendencyWalkEnd(walkIndex);
        }
    }
}