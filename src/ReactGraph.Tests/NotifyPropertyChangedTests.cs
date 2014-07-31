using System;
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
            var notifies = new Notifies
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
    }
}