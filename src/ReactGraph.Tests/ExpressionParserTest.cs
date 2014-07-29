using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ExpressionParserTest
    {
        [Fact]
        public void SimpleExpressionGetsAddedToGraph()
        {
            var graph = new DirectedGraph<NodeInfo>();
            var expressionParser = new ExpressionParser();

            var basicType = new BasicType();
            expressionParser.AddToGraph(graph, () => basicType.Target, () => TargetFormula(basicType.Source1, basicType.Source2));

            graph.EdgesCount.ShouldBe(2);
            graph.VerticiesCount.ShouldBe(3);
            graph.Verticies.ShouldContain(v => v.Data.PropertyInfo.Name == "Target" && v.Data.Instance == basicType);
            graph.Verticies.ShouldContain(v => v.Data.PropertyInfo.Name == "Source1" && v.Data.Instance == basicType);
            graph.Verticies.ShouldContain(v => v.Data.PropertyInfo.Name == "Source2" && v.Data.Instance == basicType);
        }

        private string TargetFormula(int source1, int source2)
        {
            return (source1 + source2).ToString();
        }

        public class BasicType
        {
            public string Target { get; set; }
            public int Source1 { get; set; }
            public int Source2 { get; set; }
        }
    }
}
