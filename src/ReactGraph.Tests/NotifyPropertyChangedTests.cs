using System;
using ReactGraph.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class NotifyPropertyChangedTests
    {
        readonly TestInstrumentation engineInstrumentation;
        readonly DependencyEngine engine;

        public NotifyPropertyChangedTests()
        {
            engineInstrumentation = new TestInstrumentation();
            engine = new DependencyEngine(engineInstrumentation);
        }

        [Fact]
        public void TriggersOnPropertyChanged()
        {
            var notifies = new Totals
            {
                TaxPercentage = 20
            };

            engine.Expr(() => (int)(notifies.SubTotal * (1m + (notifies.TaxPercentage / 100m))))
                  .Bind(() => notifies.Total, e => { });
            Console.WriteLine(engine.ToString());

            notifies.SubTotal = 100;

            notifies.Total.ShouldBe(120);
            engineInstrumentation.AssertSetCount("notifies.Total", 1);
        }

        [Fact]
        public void ListensToNestedProperties()
        {
            var viewModel = new MortgateCalculatorViewModel();

            engine.Expr(() => !viewModel.PaymentSchedule.HasValidationError)
                  .Bind(() => viewModel.CanApply, e => { });

            viewModel.RegeneratePaymentSchedule(hasValidationError: true);
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(false);
            engineInstrumentation.AssertSetCount("viewModel.CanApply", 1);

            viewModel.RegeneratePaymentSchedule(hasValidationError: false);
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(true);
            engineInstrumentation.AssertSetCount("viewModel.CanApply", 2);

            viewModel.PaymentSchedule.HasValidationError = true;
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(false);
            engineInstrumentation.AssertSetCount("viewModel.CanApply", 3);

            Console.WriteLine(engine.ToString());
        }

        [Fact]
        public void PreventsReentrancy()
        {
            var one = new SimpleWithNotification();
            var two = new SimpleWithNotification();
            var three = new SimpleWithNotification();
            var four = new SimpleWithNotification();

            /*     +--3<--+
             *     ▼      |
             *     4      1
             *     ^      |
             *     +--2<--+
             */
            engine.Expr(() => two.Value + three.Value)
                  .Bind(() => four.Value, e => { });
            engine.Expr(() => one.Value)
                  .Bind(() => two.Value, e => { });
            engine.Expr(() => one.Value)
                .Bind(() => three.Value, e => { });

            Console.WriteLine(engine.ToString());

            one.Value = 1;

            four.Value.ShouldBe(2);
            engineInstrumentation.AssertSetCount("four.Value", 1);
        }

        [Fact]
        public void LeafPropertiesShouldBeListenedTo()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(true);

            engine.Expr(() => CalcSomethingToDoWithSchedule(viewModel.PaymentSchedule))
                  .Bind(() => Foo, e => { });

            Foo.ShouldNotBe(42);
            viewModel.PaymentSchedule.HasValidationError = false;
            Foo.ShouldBe(42);
            engineInstrumentation.AssertSetCount("Foo", 1);
        }

        private int CalcSomethingToDoWithSchedule(ScheduleViewModel paymentSchedule)
        {
            return 42;
        }

        public int Foo { get; set; }
    }
}