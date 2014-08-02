using System;
using System.Linq.Expressions;
using ReactGraph.Internals;
using ReactGraph.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ExpressionParserTest
    {
        readonly ExpressionParser expressionParser;

        public ExpressionParserTest()
        {
            expressionParser = new ExpressionParser(new NodeRepository(new DependencyEngine()));
        }

        [Fact]
        public void GetSimpleNode()
        {
            var notifies = new Totals();
            Expression<Func<int>> expr = () => notifies.Total;
            var node = expressionParser.GetNodeInfo(expr);
            node.RootInstance.ShouldBeSameAs(notifies);
            node.ShouldBeOfType<PropertyNodeInfo<int>>()
                .PropertyInfo.ShouldBe(typeof(Totals).GetProperty("Total"));
        }

        [Fact]
        public void GetHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => viewModel.PaymentSchedule.HasValidationError;
            var node = expressionParser.GetNodeInfo(expr);
            node.RootInstance.ShouldBeSameAs(viewModel);
            node.ShouldBeOfType<PropertyNodeInfo<bool>>()
                .PropertyInfo.ShouldBe(typeof(ScheduleViewModel).GetProperty("HasValidationError"));
            node.Dependencies.Count.ShouldBe(1);
            var paymentScheduleNode = node.Dependencies[0];
            paymentScheduleNode
                .ShouldBeOfType<PropertyNodeInfo<object>>()
                .PropertyInfo.ShouldBe(typeof(MortgateCalculatorViewModel).GetProperty("PaymentSchedule"));
            paymentScheduleNode.RootInstance.ShouldBeSameAs(viewModel);
        }

        [Fact]
        public void GetInvertedHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => !viewModel.PaymentSchedule.HasValidationError;
            var node = expressionParser.GetNodeInfo(expr);
            node.RootInstance.ShouldBeSameAs(viewModel);
            node.ShouldBeOfType<FormulaExpressionInfo<bool>>();
            node.Dependencies.Count.ShouldBe(1);
            var validationErrorNode = node.Dependencies[0];
            validationErrorNode.RootInstance.ShouldBeSameAs(viewModel);
            validationErrorNode.Dependencies.Count.ShouldBe(1);
            validationErrorNode.ShouldBeOfType<PropertyNodeInfo<object>>()
                .PropertyInfo.ShouldBe(typeof(ScheduleViewModel).GetProperty("HasValidationError"));


            var paymentScheduleNode = validationErrorNode.Dependencies[0];
            paymentScheduleNode
                .ShouldBeOfType<PropertyNodeInfo<object>>()
                .PropertyInfo.ShouldBe(typeof(MortgateCalculatorViewModel).GetProperty("PaymentSchedule"));
            paymentScheduleNode.RootInstance.ShouldBeSameAs(viewModel);
        }
    }
}