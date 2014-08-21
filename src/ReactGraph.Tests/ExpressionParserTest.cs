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
        readonly ExpressionParser expressionParser;

        public ExpressionParserTest()
        {
            expressionParser = new ExpressionParser();
        }

        [Fact]
        public void GetSimpleNode()
        {
            var notifies = new Totals();
            Expression<Func<int>> expr = () => notifies.Total;
            var formulaNode = expressionParser.GetFormulaDescriptor(expr);
            formulaNode.SubExpressions.Count.ShouldBe(1);
            var propertyNode = formulaNode.SubExpressions.Single();
            propertyNode.RootInstance.ShouldBeSameAs(notifies);
            propertyNode.ShouldBeOfType<MemberSourceDescriptor<int>>()
                .MemberInfo.ShouldBe(typeof(Totals).GetProperty("Total"));
        }

        [Fact]
        public void GetHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => viewModel.PaymentSchedule.HasValidationError;
            var node = expressionParser.GetFormulaDescriptor(expr).SubExpressions.Single();
            node.RootInstance.ShouldBeSameAs(viewModel);
            node.ShouldBeOfType<MemberSourceDescriptor<bool>>()
                .MemberInfo.ShouldBe(typeof(ScheduleViewModel).GetProperty("HasValidationError"));
            node.SubExpressions.Count.ShouldBe(1);
            var paymentScheduleNode = node.SubExpressions[0];
            paymentScheduleNode
                .ShouldBeOfType<MemberSourceDescriptor<ScheduleViewModel>>()
                .MemberInfo.ShouldBe(typeof(MortgateCalculatorViewModel).GetProperty("PaymentSchedule"));
            paymentScheduleNode.RootInstance.ShouldBeSameAs(viewModel);
        }

        [Fact]
        public void GetInvertedHarderNode()
        {
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => !viewModel.PaymentSchedule.HasValidationError;
            var node = expressionParser.GetFormulaDescriptor(expr);
            node.SubExpressions.Count.ShouldBe(1);
            var validationErrorNode = node.SubExpressions[0];
            validationErrorNode.RootInstance.ShouldBeSameAs(viewModel);
            validationErrorNode.SubExpressions.Count.ShouldBe(1);
            validationErrorNode.ShouldBeOfType<MemberSourceDescriptor<bool>>()
                .MemberInfo.ShouldBe(typeof(ScheduleViewModel).GetProperty("HasValidationError"));

            var paymentScheduleNode = validationErrorNode.SubExpressions[0];
            paymentScheduleNode
                .ShouldBeOfType<MemberSourceDescriptor<ScheduleViewModel>>()
                .MemberInfo.ShouldBe(typeof(MortgateCalculatorViewModel).GetProperty("PaymentSchedule"));
            paymentScheduleNode.RootInstance.ShouldBeSameAs(viewModel);
        }

        [Fact]
        public void SimpleMethod()
        {
            var simple = new SimpleWithNotification();
            Expression<Func<int>> expr = () => Negate(simple.Value);
            var node = expressionParser.GetFormulaDescriptor(expr);
            node.SubExpressions.Count.ShouldBe(1);
        }

        int Negate(int value)
        {
            return -value;
        }
    }
}