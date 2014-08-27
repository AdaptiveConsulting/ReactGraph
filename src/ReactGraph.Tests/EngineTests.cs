using System;
using ReactGraph.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class EngineTests
    {
        readonly TestInstrumentation engineInstrumentation;
        readonly DependencyEngine engine;

        public EngineTests()
        {
            engineInstrumentation = new TestInstrumentation();
            engine = new DependencyEngine();
            engine.AddInstrumentation(engineInstrumentation);
        }

        [Fact]
        public void TracksWhenStepChanges()
        {
            var viewModel = new MortgateCalculatorViewModel();

            engine.Assign(() => viewModel.CanApply)
                  .From(() => !viewModel.PaymentSchedule.HasValidationError, e => { });

            viewModel.RegeneratePaymentSchedule(hasValidationError: true);
            engine.ValueHasChanged(viewModel, "PaymentSchedule");
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(false);
            engineInstrumentation.AssertSetCount("viewModel.CanApply", 1);

            viewModel.RegeneratePaymentSchedule(hasValidationError: false);
            engine.ValueHasChanged(viewModel, "PaymentSchedule");
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(true);
            engineInstrumentation.AssertSetCount("viewModel.CanApply", 2);

            viewModel.PaymentSchedule.HasValidationError = true;
            engine.ValueHasChanged(viewModel.PaymentSchedule, "HasValidationError");
            Console.WriteLine(engine.ToString());
            viewModel.CanApply.ShouldBe(false);
            engineInstrumentation.AssertSetCount("viewModel.CanApply", 3);

            Console.WriteLine(engine.ToString());
        }
    }
}