using System.Linq.Expressions;

namespace ReactGraph.Construction
{
    // TODO: remove? Do we use that?

    class NullCheckRewriter : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (!node.Type.IsValueType)
            {
                var nullExpr = Expression.Constant(null, node.Type);
                var test = Expression.Equal(node, nullExpr);

                var conditionalExpression = Expression.Condition(test, nullExpr, node, node.Type);
                return conditionalExpression;
            }

            if (node.Expression is MemberExpression)
            {
                var test = Expression.Equal(node.Expression, Expression.Constant(null, node.Expression.Type));

                var exceptionCtor = typeof(FormulaNullReferenceException).GetConstructor(new[]{typeof(string)});
                var errorExpression = Expression.Constant(ExpressionStringBuilder.ToString(node));
                var newException = Expression.New(exceptionCtor, errorExpression);
                var throwExpression = Expression.Throw(newException, node.Type);
                var conditionalExpression = Expression.Condition(test, throwExpression, node, node.Type);
                return conditionalExpression;
            }
            return base.VisitMember(node);
        }
    }
}