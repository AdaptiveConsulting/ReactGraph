using System;
using System.Linq;
using Xunit;

namespace ReactGraph.Tests
{
    public class GraphTests
    {
        private readonly DirectedGraph<int> _sut;

        public GraphTests()
        {
            _sut = new DirectedGraph<int>();
        }

        [Fact]
        public void PopulateGraph()
        {
            _sut.AddEdge(1, 2, "1 -> 2");
            _sut.AddEdge(1, 3, "1 -> 3");
            _sut.AddEdge(2, 3, "2 -> 3");

            Assert.Equal(3, _sut.VerticiesCount);
            Assert.Equal(3, _sut.EdgesCount);
        }

        [Fact]
        public void DeapthFirstSeach()
        {
            _sut.AddEdge(1, 2, "1 -> 2");
            _sut.AddEdge(1, 3, "1 -> 3");
            _sut.AddEdge(2, 3, "2 -> 3");
            _sut.AddEdge(3, 4, "3 -> 4");

            var result = _sut.DepthFirstSearch(2).Select(v => v.Data);

            Assert.Equal(new []{2, 3, 4}, result);
        }

        [Fact]
        public void Subgraph()
        {
            _sut.AddEdge(1, 2, "1 -> 2");
            _sut.AddEdge(1, 3, "1 -> 3");
            _sut.AddEdge(2, 3, "2 -> 3");
            _sut.AddEdge(3, 4, "3 -> 4");

            var result = _sut.SubGraph(2);

            Assert.Equal(3, result.VerticiesCount);
            Assert.Equal(2, result.EdgesCount);
        }

        [Fact]
        public void Sources()
        {
            _sut.AddEdge(1, 2, "1 -> 2");
            _sut.AddEdge(1, 3, "1 -> 3");
            _sut.AddEdge(2, 3, "2 -> 3");
            _sut.AddEdge(3, 4, "3 -> 4");
            _sut.AddEdge(5, 4, "5 -> 4");

            var result = _sut.FindSources();

            Assert.Equal(new []{1, 5}, result.Select(v => v.Data));
        }
        
        [Fact]
        public void TopologicalSort()
        {
            _sut.AddEdge(1, 2, "1 -> 2");
            _sut.AddEdge(1, 3, "1 -> 3");
            _sut.AddEdge(2, 3, "2 -> 3");
            _sut.AddEdge(3, 4, "3 -> 4");
            _sut.AddEdge(5, 4, "5 -> 4");
            _sut.AddEdge(5, 4, "2 -> 4");

            var result = _sut.TopologicalSort(1);

            Assert.Equal(new[]{ 1, 2, 3, 4 }, result.Select(v => v.Data));
        }
        
        [Fact]
        public void TopologicalSort2()
        {
            _sut.AddEdge(0, 1, "0 -> 1");
            _sut.AddEdge(0, 3, "0 -> 3");
            _sut.AddEdge(0, 4, "0 -> 4");
            _sut.AddEdge(1, 2, "1 -> 2");
            _sut.AddEdge(1, 3, "1 -> 3");
            _sut.AddEdge(3, 2, "3 -> 2");
            _sut.AddEdge(3, 4, "3 -> 4");
            _sut.AddEdge(2, 4, "2 -> 4");
            _sut.AddEdge(2, 5, "2 -> 5");
            _sut.AddEdge(4, 5, "4 -> 5");

            var result = _sut.TopologicalSort(1);

            Assert.Equal(new[]{ 1, 3, 2, 4,5 }, result.Select(v => v.Data));
        }

        [Fact]
        public void TopologicalSortThrowsOnCycle()
        {
            _sut.AddEdge(0, 1, "0 -> 1");
            _sut.AddEdge(1, 2, "1 -> 2");
            _sut.AddEdge(2, 0, "2 -> 0");

            Assert.Throws<InvalidOperationException>(() => _sut.TopologicalSort(0));
        }
    }
}
