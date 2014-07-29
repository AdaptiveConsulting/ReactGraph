using System;
using System.Linq;
using ReactGraph.Internals;
using Shouldly;
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
            _sut.AddEdge(1, 2);
            _sut.AddEdge(1, 3);
            _sut.AddEdge(2, 3);

            _sut.VerticiesCount.ShouldBe(3);
            _sut.EdgesCount.ShouldBe(3);
        }

        [Fact]
        public void DeapthFirstSeach()
        {
            _sut.AddEdge(1, 2);
            _sut.AddEdge(1, 3);
            _sut.AddEdge(2, 3);
            _sut.AddEdge(3, 4);

            var result = _sut.DepthFirstSearch(2).Select(v => v.Data);

            result.ShouldBe(new []{2, 3, 4});
        }

        [Fact]
        public void Subgraph()
        {
            _sut.AddEdge(1, 2);
            _sut.AddEdge(1, 3);
            _sut.AddEdge(2, 3);
            _sut.AddEdge(3, 4);

            var result = _sut.SubGraph(2);

            result.VerticiesCount.ShouldBe(3);
            result.EdgesCount.ShouldBe(2);
        }

        [Fact]
        public void Sources()
        {
            _sut.AddEdge(1, 2);
            _sut.AddEdge(1, 3);
            _sut.AddEdge(2, 3);
            _sut.AddEdge(3, 4);
            _sut.AddEdge(5, 4);

            var result = _sut.FindSources();

            result.Select(v => v.Data).ShouldBe(new []{1, 5});
        }
        
        [Fact]
        public void TopologicalSort()
        {
            _sut.AddEdge(1, 2);
            _sut.AddEdge(1, 3);
            _sut.AddEdge(2, 3);
            _sut.AddEdge(3, 4);
            _sut.AddEdge(5, 4);
            _sut.AddEdge(5, 4);

            var result = _sut.TopologicalSort(1);

            result.Select(v => v.Data).ShouldBe(new[] { 1, 2, 3, 4 });
        }
        
        [Fact]
        public void TopologicalSort2()
        {
            _sut.AddEdge(0, 1);
            _sut.AddEdge(0, 3);
            _sut.AddEdge(0, 4);
            _sut.AddEdge(1, 2);
            _sut.AddEdge(1, 3);
            _sut.AddEdge(3, 2);
            _sut.AddEdge(3, 4);
            _sut.AddEdge(2, 4);
            _sut.AddEdge(2, 5);
            _sut.AddEdge(4, 5);

            var result = _sut.TopologicalSort(1);

            result.Select(v => v.Data).ShouldBe(new[] {1, 3, 2, 4, 5});
        }

        [Fact]
        public void TopologicalSortThrowsOnCycle()
        {
            _sut.AddEdge(0, 1);
            _sut.AddEdge(1, 2);
            _sut.AddEdge(2, 0);

            Assert.Throws<InvalidOperationException>(() => _sut.TopologicalSort(0));
        }

        [Fact]
        public void ToDotLanguage()
        {
            _sut.AddEdge(0, 1);
            _sut.AddEdge(0, 3);
            _sut.AddEdge(0, 4);
            _sut.AddEdge(1, 2);
            _sut.AddEdge(1, 3);
            _sut.AddEdge(3, 2);
            _sut.AddEdge(3, 4);
            _sut.AddEdge(2, 4);
            _sut.AddEdge(2, 5);
            _sut.AddEdge(4, 5);

            const string expected = @"digraph Foo {
     0 -> 1;
     0 -> 3;
     0 -> 4;
     1 -> 2;
     1 -> 3;
     3 -> 2;
     3 -> 4;
     4 -> 5;
     2 -> 4;
     2 -> 5;
}";

            _sut.ToDotLanguage("Foo").ShouldBe(expected);
        }
    }
}
