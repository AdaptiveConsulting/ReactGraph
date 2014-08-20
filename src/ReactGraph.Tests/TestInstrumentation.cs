using System.Collections.Generic;
using ReactGraph.Instrumentation;
using ReactGraph.NodeInfo;

namespace ReactGraph.Tests
{
    class TestInstrumentation : IEngineInstrumentation
    {
        public TestInstrumentation()
        {
            NodeEvaluations = new List<NodeEval>();
        }

        public long WalkIndexStart { get; private set; }
        public long WalkIndexEnd { get; private set; }
        public List<NodeEval> NodeEvaluations { get; private set; }

        public void OnDependencyWalkStart(long walkIndex, string sourceProperty)
        {
            WalkIndexStart = walkIndex;
        }

        public void OnNodeEvaluated(long walkIndex, string updatedNode, ReevaluationResult result)
        {
            NodeEvaluations.Add(new NodeEval(walkIndex, updatedNode, result));
        }

        public void OnDepdendencyWalkEnd(long walkIndex)
        {
            WalkIndexEnd = walkIndex;
        }

        internal class NodeEval
        {
            public long WalkIndex { get; set; }
            public string UpdatedNode { get; set; }
            public ReevaluationResult Result { get; set; }

            public NodeEval(long walkIndex, string updatedNode, ReevaluationResult result)
            {
                WalkIndex = walkIndex;
                UpdatedNode = updatedNode;
                Result = result;
            }
        }
    }
}