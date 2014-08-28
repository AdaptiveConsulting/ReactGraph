using System;
using System.Collections.ObjectModel;
using ReactGraph.Graph;
using ReactGraph.Tests.TestObjects;
using ReactGraph.Visualisation;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ApiTest
    {
        DependencyEngine engine;

        public ApiTest()
        {
            engine = new DependencyEngine();
        }

        [Fact]
        public void FluentApiTest()
        {
            /* Each vertex is a node in this graph
             * 
             *       5<-----4
             *        \    /
             *         \  /
             *          ▼▼
             *      6   1------->3
             *      |  ^  \     ^
             *      | /    \   /
             *      ▼/      ▼ /
             *      0------->2----->7
             *               \
             *                \
             *                 ▼
             *                  8
             */
            var vertex0 = new SinglePropertyType();
            var vertex1 = new SinglePropertyType();
            var vertex2 = new SinglePropertyType();
            var vertex3 = new SinglePropertyType();
            var vertex4 = new SinglePropertyType();
            var vertex5 = new SinglePropertyType();
            var vertex6 = new SinglePropertyType();
            var vertex7 = new SinglePropertyType();
            var vertex8 = new SinglePropertyType();

            engine.Assign(() => vertex0.Value).From(() => Addition(vertex6.Value), e => { });
            engine.Assign(() => vertex1.Value).From(() => Addition(vertex0.Value, vertex5.Value, vertex4.Value), e => { });
            engine.Assign(() => vertex2.Value).From(() => Addition(vertex0.Value, vertex1.Value), e => { });
            engine.Assign(() => vertex3.Value).From(() => Addition(vertex1.Value, vertex2.Value), e => { });
            engine.Assign(() => vertex5.Value).From(() => Addition(vertex4.Value), e => { });
            engine.Assign(() => vertex7.Value).From(() => Addition(vertex2.Value), e => { });
            engine.Assign(() => vertex8.Value).From(() => Addition(vertex2.Value), e => { });
                
            Console.WriteLine(engine.ToString());

            // We set the value to 2, then tell the engine the value has changed
            vertex0.Value = 2;
            engine.ValueHasChanged(vertex0, "Value");
            vertex1.Value.ShouldBe(2);
            vertex2.Value.ShouldBe(4);
            vertex3.Value.ShouldBe(6);
        }

        [Fact]
        public void ExceptionsArePassedToHandlers()
        {
            var a = new SinglePropertyType();
            var b = new SinglePropertyType();

            Exception ex;
            engine.Assign(() => b.Value)
                  .From(() => ThrowsInvalidOperationException(a.Value), e => ex = e);

            ex = null;
            a.Value = 2;
            engine.ValueHasChanged(a, "Value");

            ex.ShouldBeOfType<InvalidOperationException>();
        }

        [Fact]
        public void NodeWhichThrowsExceptionHasSubgraphRemovedFromCurrentExecution()
        {
            /*
             *      A
             *     / \
             *    B   Throws
             *    |   |
             *    C   Skipped
             *   / \ /
             *  E   D
             */

            var a = new SinglePropertyType();
            var b = new SinglePropertyType();
            var c = new SinglePropertyType();
            var d = new SinglePropertyType();
            var e = new SinglePropertyType();
            var throws = new SinglePropertyType();
            var skipped = new SinglePropertyType();

            engine.Assign(() => b.Value).From(() => a.Value, ex => { });
            engine.Assign(() => c.Value).From(() => b.Value, ex => { });
            engine.Assign(() => throws.Value).From(() => ThrowsInvalidOperationException(a.Value), ex => { });
            engine.Assign(() => skipped.Value).From(() => throws.Value, ex => { });
            engine.Assign(() => d.Value).From(() => c.Value + skipped.Value, ex => { });
            engine.Assign(() => e.Value).From(() => c.Value, ex => { });

            a.Value = 2;
            engine.ValueHasChanged(a, "Value");

            var dotFormat = engine.ToDotFormat("Title");
            Console.WriteLine(dotFormat);

            skipped.ValueSet.ShouldBe(0);
            d.ValueSet.ShouldBe(0);
            c.Value.ShouldBe(2);
            e.Value.ShouldBe(2);
        }

        [Fact]
        public void Instrumentation()
        {
            /*
             *      A
             *     / \
             *    B   Throws
             *    |   |
             *    C   Skipped
             *   / \ /
             *  E   D
             */
            var instrumentation = new TestInstrumentation();
            engine.AddInstrumentation(instrumentation);

            var a = new SinglePropertyType();
            var b = new SinglePropertyType();
            var c = new SinglePropertyType();
            var d = new SinglePropertyType();
            var e = new SinglePropertyType();
            var throws = new SinglePropertyType();
            var skipped = new SinglePropertyType();

            engine.Assign(() => b.Value).From(() => a.Value, ex => { });
            engine.Assign(() => c.Value).From(() => b.Value, ex => { });
            engine.Assign(() => throws.Value).From(() => ThrowsInvalidOperationException(a.Value), ex => { });
            engine.Assign(() => skipped.Value).From(() => throws.Value, ex => { });
            engine.Assign(() => d.Value).From(() => c.Value + skipped.Value, ex => { });
            engine.Assign(() => e.Value).From(() => c.Value, ex => { });

            a.Value = 2;
            engine.ValueHasChanged(a, "Value");

            var dotFormat = engine.ToDotFormat("Title");
            Console.WriteLine(dotFormat);

            instrumentation.WalkIndexStart.ShouldBe(1);
            instrumentation.WalkIndexEnd.ShouldBe(1);
            instrumentation.NodeEvaluations.Count.ShouldBe(8);
        }

        [Fact]
        public void CheckCyclesShouldThrowWhenThereIsACycle()
        {
            engine = new DependencyEngine();
            var a = new SinglePropertyType();
            var b = new SinglePropertyType();

            engine.Assign(() => b.Value).From(() => a.Value * 2, ex => { });
            engine.Assign(() => a.Value).From(() => b.Value - 2, ex => { });

            var dot = engine.ToDotFormat("Title");

            Should.Throw<CycleDetectedException>(() => engine.CheckCycles())
                  .Message.ShouldBe(@"1 cycles found:
a.Value --> (a.Value * 2) --> b.Value --> (b.Value - 2) --> a.Value");
        }

        [Fact]
        public void CheckCyclesShouldThrowWhenThereAreTwoCycles()
        {
            engine = new DependencyEngine();
            var a = new SinglePropertyType();
            var b = new SinglePropertyType();
            var c = new SinglePropertyType();
            var d = new SinglePropertyType();

            engine.Assign(() => b.Value).From(() => a.Value * 2, ex => { });
            engine.Assign(() => a.Value).From(() => b.Value - 2, ex => { });

            engine.Assign(() => d.Value).From(() => c.Value * 2, ex => { });
            engine.Assign(() => c.Value).From(() => d.Value - 2, ex => { });

            Should.Throw<CycleDetectedException>(() => engine.CheckCycles())
                  .Message.ShouldBe(@"2 cycles found:
a.Value --> (a.Value * 2) --> b.Value --> (b.Value - 2) --> a.Value
c.Value --> (c.Value * 2) --> d.Value --> (d.Value - 2) --> c.Value");
        }

        [Fact]
        public void CheckCyclesShouldNotThrowWhenNoCycleExists()
        {
            engine = new DependencyEngine();
            var a = new SinglePropertyType();
            var b = new SinglePropertyType();

            engine.Assign(() => b.Value).From(() => a.Value * 2, ex => { });

            Should.NotThrow(() => engine.CheckCycles());
        }

        [Fact]
        public void TracksNestedProperties()
        {
            var mortgateCalculator = new MortgateCalculatorViewModel();
            mortgateCalculator.RegeneratePaymentSchedule(false);

            engine
                .Assign(() => Prop)
                .From(() => mortgateCalculator.PaymentSchedule.HasValidationError, ex => { });

            mortgateCalculator.PaymentSchedule.HasValidationError = true;

            engine.ValueHasChanged(mortgateCalculator.PaymentSchedule, "HasValidationError");
            Prop.ShouldBe(true);
        }

        public bool Prop { get; set; }

        [Fact]
        public void CanUseCurrentValueWhenRecalculating()
        {
            var optionsViewModel = new OptionsViewModel
            {
                Options = new ObservableCollection<string>
                {
                    "Item 1",
                    "Item 2",
                    "Item 3"
                },
                SelectedOption = "Item 1"
            };

            engine
                .Assign(() => optionsViewModel.SelectedOption)
                .From(currentValue => UnselectInvalidOption(currentValue, optionsViewModel.Options), ex => { });

            optionsViewModel.Options = new ObservableCollection<string>
            {
                "Item 1",
                "Item 2"
            };
            optionsViewModel.SelectedOption.ShouldBe("Item 1");

            optionsViewModel.Options = new ObservableCollection<string>
            {
                "Item 2",
                "Item 3"
            };
            optionsViewModel.SelectedOption.ShouldBe(null);
        }

        string UnselectInvalidOption(string currentValue, ObservableCollection<string> options)
        {
            if (!options.Contains(currentValue))
                return null;

            return currentValue;
        }

        int ThrowsInvalidOperationException(int value)
        {
            throw new InvalidOperationException();
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
