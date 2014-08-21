using System;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Construction;
using ReactGraph.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ExpressionParserTest
    {
        [Fact]
        public void GetSimpleNode()
        {
            var notifies = new Totals();
            Expression<Func<int>> expr = () => notifies.Total;
            var root = ExpressionParser.GetRootOf(expr);
            root.ShouldBeSameAs(notifies);
            var subExpressions = ExpressionParser.GetChildSources(expr, root);
            subExpressions.Count.ShouldBe(1);
            var propertyNode = subExpressions.Single();
            propertyNode.Root.ShouldBeSameAs(notifies);
            propertyNode.Path.ShouldBe("notifies.Total");
        }

        [Fact]
        public void GetHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => viewModel.PaymentSchedule.HasValidationError;
            var root = ExpressionParser.GetRootOf(expr);
            root.ShouldBeSameAs(viewModel);
            var node = ExpressionParser.GetChildSources(expr, root).Single();
            node.Root.ShouldBeSameAs(viewModel);
            node.Path.ShouldBe("viewModel.PaymentSchedule.HasValidationError");
            node.SourcePaths.Count.ShouldBe(1);
            var paymentScheduleNode = node.SourcePaths[0];
            paymentScheduleNode.Path.ShouldBe("viewModel.PaymentSchedule");
            paymentScheduleNode.Root.ShouldBeSameAs(viewModel);
        }

        [Fact]
        public void GetInvertedHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => !viewModel.PaymentSchedule.HasValidationError;
            var root = ExpressionParser.GetRootOf(expr);
            root.ShouldBeSameAs(viewModel);
            var node = ExpressionParser.GetChildSources(expr, root);
            node.Count.ShouldBe(1);
            var validationErrorNode = node[0];
            validationErrorNode.Root.ShouldBeSameAs(viewModel);
            validationErrorNode.Path.ShouldBe("viewModel.PaymentSchedule.HasValidationError");
            validationErrorNode.SourcePaths.Count.ShouldBe(1);

            var paymentScheduleNode = validationErrorNode.SourcePaths[0];
            paymentScheduleNode.Path.ShouldBe("viewModel.PaymentSchedule");
            paymentScheduleNode.Root.ShouldBeSameAs(viewModel);
        }

        [Fact]
        public void SimpleMethod()
        {
            var simple = new SimpleWithNotification();
            Expression<Func<int>> expr = () => Negate(simple.Value);
            var root = ExpressionParser.GetRootOf(expr);
            root.ShouldBeSameAs(simple);
            var node = ExpressionParser.GetChildSources(expr, root);
            node.Count.ShouldBe(1);
        }

        int Negate(int value)
        {
            return -value;
        }
    }
}