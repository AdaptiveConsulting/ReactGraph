using System;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Internals;
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
            var expressionParser = new ExpressionParser();
            var notifies = new Totals();
            Expression<Func<int>> expr = () => notifies.Total;
            var node = expressionParser.GetFormulaExpressionInfo(expr);
            node.Single().RootInstance.ShouldBeSameAs(notifies);
            node.Single().PropertyInfo.ShouldBe(typeof(Totals).GetProperty("Total"));
        }

        [Fact]
        public void GetHarderNode()
        {
            var expressionParser = new ExpressionParser();
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            Expression<Func<bool>> expr = () => viewModel.PaymentSchedule.HasValidationError;
            var node = expressionParser.GetFormulaExpressionInfo(expr).Single();
            node.RootInstance.ShouldBeSameAs(viewModel);
            node.PropertyInfo.ShouldBe(typeof(ScheduleViewModel).GetProperty("HasValidationError"));
            var parent = node.PropertyExpression.Expression;

            var parentNode = expressionParser.GetFormulaExpressionInfo(parent).Single();
            parentNode.RootInstance.ShouldBeSameAs(viewModel);
            parentNode.PropertyInfo.ShouldBe(typeof(MortgateCalculatorViewModel).GetProperty("PaymentSchedule"));
        }

        [Fact]
        public void GetInvertedHarderNode()
        {
            var expressionParser = new ExpressionParser();
            var viewModel = new MortgateCalculatorViewModel();
            viewModel.RegeneratePaymentSchedule(false);
            var expr = GetExpression(() => !viewModel.PaymentSchedule.HasValidationError);
            var node = expressionParser.GetFormulaExpressionInfo(expr).Single();
            node.RootInstance.ShouldBeSameAs(viewModel);
            node.PropertyInfo.ShouldBe(typeof(ScheduleViewModel).GetProperty("HasValidationError"));
            var parent = node.PropertyExpression.Expression;

            var parentNode = expressionParser.GetFormulaExpressionInfo(parent).Single();
            parentNode.RootInstance.ShouldBeSameAs(viewModel);
            parentNode.PropertyInfo.ShouldBe(typeof(MortgateCalculatorViewModel).GetProperty("PaymentSchedule"));
        }

        private Expression GetExpression<TProp>(Expression<Func<TProp>> func)
        {
            return func;
        }
    }
}