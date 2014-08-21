using System.Collections.Generic;
using ReactGraph.NodeInfo;

namespace ReactGraph.Instrumentation
{
    class EngineInstrumenter
    {
        readonly List<IEngineInstrumentation> instrumentations = new List<IEngineInstrumentation>(); 
        long walkIndex;

        public void DependecyWalkStarted(string sourceProperty, string nodeId)
        {
            walkIndex++;

            foreach (var engineInstrumentation in instrumentations)
            {
                engineInstrumentation.OnDependencyWalkStart(walkIndex, sourceProperty, nodeId);
            }
        }

        public void NodeEvaluated(string updatedNode, string nodeId, ReevaluationResult result)
        {
            foreach (var engineInstrumentation in instrumentations)
            {
                engineInstrumentation.OnNodeEvaluated(walkIndex, updatedNode, nodeId, result);
            }
        }

        public void DependencyWalkEnded()
        {
            foreach (var engineInstrumentation in instrumentations)
            {
                engineInstrumentation.OnDepdendencyWalkEnd(walkIndex);
            }
        }

        public void Add(IEngineInstrumentation engineInstrumentation)
        {
            instrumentations.Add(engineInstrumentation);
        }

        public void Remove(IEngineInstrumentation engineInstrumentation)
        {
            instrumentations.Remove(engineInstrumentation);
        }
    }
}