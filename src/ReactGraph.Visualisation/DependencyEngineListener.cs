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
        readonly bool indent;
        DirectedGraph<INodeMetadata> graphSnapshot;
        readonly Dictionary<string, NodeDetails> nodesInPath = new Dictionary<string, NodeDetails>();
        int nodeIndex;

        public DependencyEngineListener(DependencyEngine dependencyEngine, Action<string> onWalkComplete, bool indent = true)
        {
            this.dependencyEngine = dependencyEngine;
            this.onWalkComplete = onWalkComplete;
            this.indent = indent;
            dependencyEngine.AddInstrumentation(this);
        }

        public void OnDependencyWalkStart(long walkIndex, string sourceProperty, string nodeId)
        {
            graphSnapshot = dependencyEngine.GetGraphSnapshot();
            nodesInPath.Clear();
            nodesInPath[nodeId] = new NodeDetails(ReevaluationResult.Changed, 1);
            nodeIndex = 2;
        }

        public void OnNodeEvaluated(long walkIndex, string updatedNode, string nodeId, ReevaluationResult result)
        {
            nodesInPath[nodeId] = new NodeDetails(result, nodeIndex++);
        }

        public void OnDependendencyWalkEnd(long walkIndex)
        {
            var visualisation = new DotVisualisation(graphSnapshot);
            onWalkComplete(visualisation.Generate("Transition" + walkIndex, prop =>
            {
                NodeDetails nodeDetails;
                if (nodesInPath.TryGetValue(prop.Id, out nodeDetails))
                {
                    switch (nodeDetails.Result)
                    {
                        case ReevaluationResult.NoChange:
                            prop.Color = "khaki1";
                            break;
                        case ReevaluationResult.Error:
                            prop.Color = "firebrick1";
                            break;
                        case ReevaluationResult.Changed:
                            prop.Color = "palegreen";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    prop.Label = "[" + nodeDetails.Index + "] " + prop.Label;
                }
                else
                {
                    prop.Color = "gray";
                }
                return prop;
            }, indent));
        }

        public void Dispose()
        {
            dependencyEngine.RemoveInstrumentation(this);
        }

        class NodeDetails
        {
            public ReevaluationResult Result { get; private set; }
            public int Index { get; private set; }

            public NodeDetails(ReevaluationResult result, int index)
            {
                Result = result;
                Index = index;
            }
        }
    }
}