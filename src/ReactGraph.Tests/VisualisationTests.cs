using System;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class VisualisationTests
    {
        readonly DependencyEngine engine;

        public VisualisationTests()
        {
            engine = new DependencyEngine();
        }

        [Fact]
        public void AddMetadataToNodesTest()
        {
            /*
             * A -> A + B ----> C
             *      ^
             *     /
             *    /
             *   B
             */

            var a = new SinglePropertyType();
            var b = new SinglePropertyType();
            var c = new SinglePropertyType();
            b.Value = 3;

            const string additionId = "Addition";
            const string propertyId = "C";
            engine.Expr(() => Addition(a.Value, b.Value), additionId)
                  .Bind(() => c.Value, e => { }, propertyId);

            var dotFormat = engine.Visualisation.Generate("Foo",
                prop =>
                {
                    switch (prop.Id)
                    {
                        case additionId:
                            prop.Color = ".7 .3 1.0";
                            prop.Label = "+";
                            break;
                        case propertyId:
                            prop.Color = ".7 .3 .5";
                            break;
                    }

                    return prop;
                });

            Console.WriteLine(dotFormat);

            // We set the value to 2, then tell the engine the value has changed
            a.Value = 2;
            engine.ValueHasChanged(a, "Value");
            c.Value.ShouldBe(5);

            var lines = dotFormat.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            lines[2].ShouldContain("[label=\"+\", fillcolor=\".7 .3 1.0\"]");
            lines[4].ShouldContain("[label=\"c.Value\", fillcolor=\".7 .3 .5\"]");
            lines[1].ShouldContain("[label=\"a.Value\"]");
        }

        private int Addition(int i1, int i2, int i3)
        {
            return i1 + i2 + i3;
        }

        private int Addition(int i1, int i2)
        {
            return i1 + i2;
        }

        private int Addition(int i1)
        {
            return i1;
        }

        private class SinglePropertyType
        {
            int value;

            public int Value
            {
                get { return value; }
                set
                {
                    this.value = value;
                    ValueSet++;
                }
            }

            public int ValueSet { get; private set; }
        }
    }
}