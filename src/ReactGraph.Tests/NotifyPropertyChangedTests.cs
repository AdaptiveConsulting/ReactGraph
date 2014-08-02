using System;
using System.Collections.Generic;
using ReactGraph.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class NotifyPropertyChangedTests
    {
        private readonly DependencyEngine engine;

        public NotifyPropertyChangedTests()
        {
            engine = new DependencyEngine();
        }

        [Fact]
        public void TriggersOnPropertyChanged()
        {
            var notifies = new Totals
            {
                TaxPercentage = 20
            };

            engine.Bind(() => notifies.Total, () => (int)(notifies.SubTotal * (1m + (notifies.TaxPercentage / 100m))));

            notifies.SubTotal = 100;
            notifies.Total.ShouldBe(120);
        }

        [Fact]
        public void ListensToNestedProperties()
        {
            var viewModel = new MortgateCalculatorViewModel();

            engine.Bind(() => viewModel.CanApply, () => !viewModel.PaymentSchedule.HasValidationError);

            viewModel.RegeneratePaymentSchedule(hasValidationError: true);
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(false);

            viewModel.RegeneratePaymentSchedule(hasValidationError: false);
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(true);

            viewModel.PaymentSchedule.HasValidationError = true;
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(false);

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
            engine.Bind(() => four.Value, () => two.Value + three.Value);
            engine.Bind(() => two.Value, () => one.Value);
            engine.Bind(() => three.Value, () => one.Value);

            var updatedObjects = new List<SimpleWithNotification>();
            engine.SettingValue += (o, s) => updatedObjects.Add((SimpleWithNotification) o);

            one.Value = 1;

            four.Value.ShouldBe(2);
            updatedObjects.Count.ShouldBe(3);
            updatedObjects.ShouldContain(two);
            updatedObjects.ShouldContain(three);
            updatedObjects.ShouldContain(four);
        }

        [Fact]
        public void LeafPropertiesShouldBeListenedTo()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(true);

            engine.Bind(() => Foo, () => CalcSomethingToDoWithSchedule(viewModel.PaymentSchedule));

            Foo.ShouldNotBe(42);
            viewModel.PaymentSchedule.HasValidationError = false;
            Foo.ShouldBe(42);
        }

        private int CalcSomethingToDoWithSchedule(ScheduleViewModel paymentSchedule)
        {
            return 42;
        }

        public int Foo { get; set; }
    }
}