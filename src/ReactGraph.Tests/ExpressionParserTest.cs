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
            var subExpressions = ExpressionParser.GetChildSources(expr);
            subExpressions.Count.ShouldBe(1);
            var propertyNode = subExpressions.Single();
            propertyNode.Path.ShouldBe("notifies.Total");
            propertyNode.SourcePaths.Count.ShouldBe(1);
            var notifiesNode = propertyNode.SourcePaths[0];
            notifiesNode.Path.ShouldBe("notifies");
        }

        [Fact]
        public void GetHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => viewModel.PaymentSchedule.HasValidationError;
            var node = ExpressionParser.GetChildSources(expr).Single();
            node.Path.ShouldBe("viewModel.PaymentSchedule.HasValidationError");
            node.SourcePaths.Count.ShouldBe(1);
            var paymentScheduleNode = node.SourcePaths[0];
            paymentScheduleNode.Path.ShouldBe("viewModel.PaymentSchedule");

            var rootNode = paymentScheduleNode.SourcePaths.Single();
            rootNode.Path.ShouldBe("viewModel");
            rootNode.SourcePaths.ShouldBeEmpty();
        }

        [Fact]
        public void GetInvertedHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => !viewModel.PaymentSchedule.HasValidationError;
            var node = ExpressionParser.GetChildSources(expr);
            node.Count.ShouldBe(1);
            var validationErrorNode = node[0];
            validationErrorNode.Path.ShouldBe("viewModel.PaymentSchedule.HasValidationError");
            validationErrorNode.SourcePaths.Count.ShouldBe(1);

            var paymentScheduleNode = validationErrorNode.SourcePaths[0];
            paymentScheduleNode.Path.ShouldBe("viewModel.PaymentSchedule");

            var rootNode = paymentScheduleNode.SourcePaths.Single();
            rootNode.Path.ShouldBe("viewModel");
            rootNode.SourcePaths.ShouldBeEmpty();
        }

        [Fact]
        public void SimpleMethod()
        {
            var simple = new SimpleWithNotification();
            Expression<Func<int>> expr = () => Negate(simple.Value);
            var node = ExpressionParser.GetChildSources(expr);
            node.Count.ShouldBe(1);
            var valueNode = node.Single();
            valueNode.Path.ShouldBe("simple.Value");
            var rootNode = valueNode.SourcePaths.Single();
            rootNode.Path.ShouldBe("simple");
            rootNode.SourcePaths.ShouldBeEmpty();
        }

        [Fact]
        public void SimpleParamsMethod()
        {
            var simple = new SimpleWithNotification();
            var simple2 = new SimpleWithNotification();
            Expression<Func<int>> expr = () => Add(simple.Value, simple2.Value);
            var node = ExpressionParser.GetChildSources(expr);
            node.Count.ShouldBe(2);
            var valueNode = node[0];
            valueNode.Path.ShouldBe("simple.Value");
            var rootNode = valueNode.SourcePaths.Single();
            rootNode.Path.ShouldBe("simple");
            rootNode.SourcePaths.ShouldBeEmpty();
            var valueNode2 = node[1];
            valueNode2.Path.ShouldBe("simple2.Value");
            var rootNode2 = valueNode2.SourcePaths.Single();
            rootNode2.Path.ShouldBe("simple2");
            rootNode2.SourcePaths.ShouldBeEmpty();
        }

        int Add(int value, int value2)
        {
            return value + value2;
        }

        int Negate(int value)
        {
            return -value;
        }
    }
}