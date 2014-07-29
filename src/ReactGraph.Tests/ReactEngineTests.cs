using System;
using System.Linq.Expressions;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ReactEngineTests
    {
        [Fact]
        public void ReevaluatesSimpleDependency()
        {
            var graph = new DirectedGraph<NodeInfo>();
            var expressionParser = new ExpressionParser();
            var basicType = new BasicType
            {
                Source1 = 1,
                Source2 = 2
            };
            expressionParser.AddToGraph(graph, () => basicType.Target, () => TargetFormula(basicType.Source1, basicType.Source2));
            var engine = new ReactEngine(graph);

            engine.PropertyChanged(basicType, "Source1");

            basicType.Target.ShouldBe("3");
        }

        private string TargetFormula(int source1, int source2)
        {
            return (source1 + source2).ToString();
        }
    }
}