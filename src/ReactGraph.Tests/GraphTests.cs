using System;
using System.Linq;
using ReactGraph.Internals;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class GraphTests
    {
        private readonly DirectedGraph<int> sut;

        public GraphTests()
        {
            sut = new DirectedGraph<int>();
        }

        [Fact]
        public void PopulateGraph()
        {
            sut.AddEdge(1, 2);
            sut.AddEdge(1, 3);
            sut.AddEdge(2, 3);
            PrintGraphToConsole("PopulateGraph");

            sut.VerticiesCount.ShouldBe(3);
            sut.EdgesCount.ShouldBe(3);
        }

        [Fact]
        public void DeapthFirstSeach()
        {
            sut.AddEdge(1, 2);
            sut.AddEdge(1, 3);
            sut.AddEdge(2, 3);
            sut.AddEdge(3, 4);
            PrintGraphToConsole("DeapthFirstSeach");

            var result = sut.DepthFirstSearch(2).Select(v => v.Data);

            result.ShouldBe(new []{2, 3, 4});
        }

        [Fact]
        public void Subgraph()
        {
            sut.AddEdge(1, 2);
            sut.AddEdge(1, 3);
            sut.AddEdge(2, 3);
            sut.AddEdge(3, 4);
            PrintGraphToConsole("graph");

            var result = sut.SubGraph(2);

            result.VerticiesCount.ShouldBe(3);
            result.EdgesCount.ShouldBe(2);
        }

        [Fact]
        public void Sources()
        {
            sut.AddEdge(1, 2);
            sut.AddEdge(1, 3);
            sut.AddEdge(2, 3);
            sut.AddEdge(3, 4);
            sut.AddEdge(5, 4);
            PrintGraphToConsole("Sources");

            var result = sut.FindSources();

            result.Select(v => v.Data).ShouldBe(new []{1, 5});
        }
        
        [Fact]
        public void TopologicalSort()
        {
            sut.AddEdge(1, 2);
            sut.AddEdge(1, 3);
            sut.AddEdge(2, 3);
            sut.AddEdge(3, 4);
            sut.AddEdge(5, 4);
            sut.AddEdge(5, 4);

            var result = sut.TopologicalSort(1);
            PrintGraphToConsole("TopologicalSort");

            result.Select(v => v.Data).ShouldBe(new[] { 1, 2, 3, 4 });
        }
        
        [Fact]
        public void TopologicalSort2()
        {
            sut.AddEdge(0, 1);
            sut.AddEdge(0, 3);
            sut.AddEdge(0, 4);
            sut.AddEdge(1, 2);
            sut.AddEdge(1, 3);
            sut.AddEdge(3, 2);
            sut.AddEdge(3, 4);
            sut.AddEdge(2, 4);
            sut.AddEdge(2, 5);
            sut.AddEdge(4, 5);
            PrintGraphToConsole("TopologicalSort2");

            var result = sut.TopologicalSort(1);

            result.Select(v => v.Data).ShouldBe(new[] {1, 3, 2, 4, 5});
        }

        [Fact]
        public void TopologicalSortThrowsOnCycle()
        {
            sut.AddEdge(0, 1);
            sut.AddEdge(1, 2);
            sut.AddEdge(2, 0);
            PrintGraphToConsole("TopologicalSortThrowsOnCycle");

            Assert.Throws<InvalidOperationException>(() => sut.TopologicalSort(0));
        }

        [Fact]
        public void ToDotLanguage()
        {
            sut.AddEdge(0, 1);
            sut.AddEdge(0, 3);
            sut.AddEdge(0, 4);
            sut.AddEdge(1, 2);
            sut.AddEdge(1, 3);
            sut.AddEdge(3, 2);
            sut.AddEdge(3, 4);
            sut.AddEdge(2, 4);
            sut.AddEdge(2, 5);
            sut.AddEdge(4, 5);
            PrintGraphToConsole("ToDotLanguage");

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

            sut.ToDotLanguage("Foo").ShouldBe(expected);
        }

        [Fact]
        public void CycleDetection()
        {
            // cycle 1
            sut.AddEdge(0, 1);
            sut.AddEdge(1, 2);
            sut.AddEdge(2, 0);

            sut.AddEdge(2, 3);

            // cycle 2
            sut.AddEdge(3, 4);
            sut.AddEdge(4, 3);

            PrintGraphToConsole("CycleDetection");

            var detectedCyles = sut.DetectCyles().ToList();

            foreach (var cyle in detectedCyles)
            {
                Console.WriteLine("Cycle: " + string.Join(", ", cyle.Select(v => v.Data.ToString())));
            }

            detectedCyles.Count().ShouldBe(2);
        }

        private void PrintGraphToConsole(string name)
        {
            Console.WriteLine(sut.ToDotLanguage(name));
        }
    }
}
