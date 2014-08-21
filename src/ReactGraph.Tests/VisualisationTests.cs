using System;
using System.IO;
using ApprovalTests;
using ReactGraph.Tests.TestObjects;
using ReactGraph.Visualisation;
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
            engine.Assign(() => c.Value, propertyId)
                  .From(() => Addition(a.Value, b.Value), e => { }, additionId);

            var dotFormat = engine.ToDotFormat("Foo",
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

            Approvals.Verify(dotFormat);
        }

        [Fact]
        public void DefaultColoursForDifferentNodes()
        {
            var notifies = new Totals
            {
                TaxPercentage = 20
            };

            engine.Assign(() => notifies.Total)
                  .From(() => (int)(notifies.SubTotal * (1m + (notifies.TaxPercentage / 100m))), e => { });

            var dotFormat = engine.ToDotFormat(string.Empty);

            Approvals.Verify(dotFormat);
        }

        [Fact]
        public void LogAllTransitionsAsDot()
        {
            var foo = new Foo();

            engine.Expr(() => foo.A + foo.B)
                  .Bind(() => foo.C, e => { });
            engine.Expr(() => foo.A + foo.C)
                  .Bind(() => foo.D, e => { });

            var temp = string.Empty;
            var disposable = engine.OnWalkComplete(s => { temp = s; });

            foo.C = 4;
            engine.ValueHasChanged(foo, "C");

            Approvals.Verify(temp);

            disposable.Dispose();

            temp = string.Empty;
            foo.C = 5;
            engine.ValueHasChanged(foo, "C");

            temp.ShouldBeNullOrEmpty();
        }

        [Fact]
        public void LogTransitionsInDotFormat()
        {
            var foo = new Foo();

            engine.Expr(() => foo.A + foo.B)
                  .Bind(() => foo.C, e => { });
            engine.Expr(() => foo.A + foo.C)
                  .Bind(() => foo.D, e => { });

            var path = Path.Combine(Environment.CurrentDirectory, "Transitions.log");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var disposable = engine.LogTransitionsInDotFormat(path);

            foo.C = 4;
            engine.ValueHasChanged(foo, "C");
            foo.A = 2;
            engine.ValueHasChanged(foo, "A");
            disposable.Dispose();

            Approvals.Verify(File.ReadAllText(path));
        }

        private int Addition(int i1, int i2)
        {
            return i1 + i2;
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

        private class SampleViewModel
        {
            public SampleViewModel(SubViewModel subViewModel, Model model)
            {
                SubViewModel = subViewModel;
                Model = model;
            }

            public SubViewModel SubViewModel { get; set; }

            public Model Model { get; set; }
        }

        class Model
        {
            public Model(string hello, string world)
            {
                Hello = hello;
                World = world;
            }

            public string Hello { get; set; }

            public string World { get; set; }
        }

        class SubViewModel
        {
            public SubViewModel(string computedProperty)
            {
                ComputedProperty = computedProperty;
            }

            public string ComputedProperty { get; set; }
        }

        class Foo
        {
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
            public int D { get; set; }
        }
    }
}