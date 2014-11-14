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
            propertyNode.FullPath.ShouldBe("notifies.Total");
            propertyNode.SourcePaths.Count.ShouldBe(1);
            var notifiesNode = propertyNode.SourcePaths[0];
            notifiesNode.FullPath.ShouldBe("notifies");
        }

        [Fact]
        public void GetHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => viewModel.PaymentSchedule.HasValidationError;
            var node = ExpressionParser.GetChildSources(expr).Single();
            node.FullPath.ShouldBe("viewModel.PaymentSchedule.HasValidationError");
            node.SourcePaths.Count.ShouldBe(1);
            var paymentScheduleNode = node.SourcePaths[0];
            paymentScheduleNode.FullPath.ShouldBe("viewModel.PaymentSchedule");

            var rootNode = paymentScheduleNode.SourcePaths.Single();
            rootNode.FullPath.ShouldBe("viewModel");

            var thisNode = rootNode.SourcePaths.Single();
            thisNode.FullPath.ShouldBe("this");
            thisNode.SourcePaths.ShouldBeEmpty();
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
            validationErrorNode.FullPath.ShouldBe("viewModel.PaymentSchedule.HasValidationError");
            validationErrorNode.SourcePaths.Count.ShouldBe(1);

            var paymentScheduleNode = validationErrorNode.SourcePaths[0];
            paymentScheduleNode.FullPath.ShouldBe("viewModel.PaymentSchedule");

            var rootNode = paymentScheduleNode.SourcePaths.Single();
            rootNode.FullPath.ShouldBe("viewModel");

            var thisNode = rootNode.SourcePaths.Single();
            thisNode.FullPath.ShouldBe("this");
            thisNode.SourcePaths.ShouldBeEmpty();
        }

        [Fact]
        public void SimpleMethod()
        {
            var simple = new SimpleWithNotification();
            Expression<Func<int>> expr = () => Negate(simple.Value);
            var node = ExpressionParser.GetChildSources(expr);
            node.Count.ShouldBe(1);
            var valueNode = node.Single();
            valueNode.FullPath.ShouldBe("simple.Value");
            var rootNode = valueNode.SourcePaths.Single();
            rootNode.FullPath.ShouldBe("simple");

            var thisNode = rootNode.SourcePaths.Single();
            thisNode.FullPath.ShouldBe("this");
            thisNode.SourcePaths.ShouldBeEmpty();
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
            valueNode.FullPath.ShouldBe("simple.Value");
            var rootNode = valueNode.SourcePaths.Single();
            rootNode.FullPath.ShouldBe("simple");

            var thisNode = rootNode.SourcePaths.Single();
            thisNode.FullPath.ShouldBe("this");
            thisNode.SourcePaths.ShouldBeEmpty();

            var valueNode2 = node[1];
            valueNode2.FullPath.ShouldBe("simple2.Value");
            var rootNode2 = valueNode2.SourcePaths.Single();
            rootNode2.FullPath.ShouldBe("simple2");

            thisNode = rootNode2.SourcePaths.Single();
            thisNode.FullPath.ShouldBe("this");
            thisNode.SourcePaths.ShouldBeEmpty();
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