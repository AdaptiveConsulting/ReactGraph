using System;
using System.Collections.Generic;
using ReactGraph.Graph;
using ReactGraph.Instrumentation;
using ReactGraph.NodeInfo;

namespace ReactGraph.Visualisation
{
    public class DependencyEngineListener : IEngineInstrumentation, IDisposable
    {
        readonly DependencyEngine dependencyEngine;
        readonly Action<string> onWalkComplete;
        DirectedGraph<INodeMetadata> graphSnapshot;
        List<string> nodeIds = new List<string>(); 

        public DependencyEngineListener(DependencyEngine dependencyEngine, Action<string> onWalkComplete)
        {
            this.dependencyEngine = dependencyEngine;
            this.onWalkComplete = onWalkComplete;
            dependencyEngine.AddInstrumentation(this);
        }

        public void OnDependencyWalkStart(long walkIndex, string sourceProperty, string nodeId)
        {
            graphSnapshot = dependencyEngine.GetGraphSnapshot();
            nodeIds = new List<string> {nodeId};
        }

        public void OnNodeEvaluated(long walkIndex, string updatedNode, string nodeId, ReevaluationResult result)
        {
            nodeIds.Add(nodeId);
        }

        public void OnDepdendencyWalkEnd(long walkIndex)
        {
            var visualisation = new DotVisualisation(graphSnapshot);
            onWalkComplete(visualisation.Generate("Transition" + walkIndex, prop =>
            {
                if (nodeIds.Contains(prop.Id))
                {
                    var index = nodeIds.IndexOf(prop.Id);
                    prop.Color = "palegreen";
                    prop.Label = "[" + index + "] " + prop.Label;
                }
                else
                {
                    prop.Color = "gray";                    
                }
                return prop;
            }));
        }

        public void Dispose()
        {
            dependencyEngine.RemoveInstrumentation(this);
        }
    }
}