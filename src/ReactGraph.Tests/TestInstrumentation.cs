using System.Collections.Generic;
using System.Linq;
using ReactGraph.Instrumentation;
using ReactGraph.NodeInfo;
using Shouldly;

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

        public void OnDependencyWalkStart(long walkIndex, string sourceProperty, string nodeId)
        {
            WalkIndexStart = walkIndex;
        }

        public void OnNodeEvaluated(long walkIndex, string updatedNode, string nodeId, ReevaluationResult result)
        {
            NodeEvaluations.Add(new NodeEval(walkIndex, updatedNode, result));
        }

        public void OnDependendencyWalkEnd(long walkIndex)
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

        public void AssertSetCount(string property, int count)
        {
            var nodeEvaluationCount = NodeEvaluations.Where(n => n.UpdatedNode == property);

            nodeEvaluationCount.Count().ShouldBe(count);
        }
    }
}