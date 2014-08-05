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

//        [Fact]
//        public void UserClustersToRepresentParents()
//        {
//            var subViewModel = new SubViewModel(string.Empty);
//            var model = new Model("Hello ", "World!");
//            var vm = new SampleViewModel(subViewModel, model);

//            engine.Expr(() => vm.Model.Hello + vm.Model.World)
//                  .Bind(() => vm.SubViewModel.ComputedProperty, e => { });

//            var dotFormat = engine.Visualisation.Generate("Foo", prop => prop, showRootAsClusters: true);

//            Console.WriteLine(dotFormat);

//            const string expected = @"digraph Foo {
//     compounded=true;
//     subgraph cluster0 {
//          label=vm.SubViewModel;
//          __5;
//     }
//     subgraph cluster1 {
//          label=vm.Model;
//          __1;
//          __4;
//     }
//
//     __1 [label=""vm.Model.Hello""];
//     __2 [label=""() => vm.Model.Hello + vm.Model.World];
//     __4 [label=""vm.Model.World""];
//     __5 [label=""vm.SubViewModel.ComputedProperty""];
//
//     __1 -> __2;
//     __2 -> __5;
//     __4 -> __2;
//}";

//            dotFormat.ShouldBe(expected);

//            /*
//             * digraph G {
//                compound=true;
//                subgraph cluster0 {
//                    a -> b;
//                    a -> c;
//                    b -> d;
//                    c -> d;
//                }
//                subgraph cluster1 {
//                    e -> g;
//                    e -> f;
//                }
//                b -> f [lhead=cluster1];
//                d -> e;
//                c -> g [ltail=cluster0,
//                lhead=cluster1];
//                c -> e [ltail=cluster0];
//                d -> h;
//                }
//                */
//        }

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

            public int Value1 { get; set; }
            public int Value2 { get; set; }
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
    }
}