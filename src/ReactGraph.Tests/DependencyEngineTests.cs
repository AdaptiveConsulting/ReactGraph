using System;
using ReactGraph.Tests.TestObjects;
using ReactGraph.Visualisation;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class DependencyEngineTests : NotifyPropertyChanged
    {
        [Fact]
        public void LocalPropertiesCanBeUsed()
        {
            var dependencyEngine = new DependencyEngine();
            dependencyEngine.Assign(() => C).From(() => Add(A, B), ex => { });

            Console.Write(dependencyEngine.ToDotFormat());

            A = 1;
            dependencyEngine.ValueHasChanged(this, "A");
            B = 1;
            dependencyEngine.ValueHasChanged(this, "B");

            C.ShouldBe(2);
        }

        int Add(int i, int i1)
        {
            return i + i1;
        }

        public int A { get; set; }

        public int B { get; set; }

        public int C { get; set; }
    }
}