using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph
{
    public class ExpressionParser
    {
        public void AddToGraph<TProp>(DirectedGraph<NodeInfo> graph, Expression<Func<TProp>> target, Expression<Func<TProp>> sourceFunction)
        {
            var targetVertex = GetNodeInfo(target);
            var dependentNodes = GetDependentNodeInfos(sourceFunction);

            foreach (var dependentNode in dependentNodes)
            {
                graph.AddEdge(targetVertex, dependentNode, string.Empty);
            }
        }

        private NodeInfo[] GetDependentNodeInfos<TProp>(Expression<Func<TProp>> formula)
        {
            var body = (MethodCallExpression)formula.Body;
            return body.Arguments.SelectMany(a =>
            {
                var propertyExpression = a as MemberExpression;
                if (propertyExpression != null)
                {
                    var expression = propertyExpression.Expression as MemberExpression;
                    var constantExpression = (ConstantExpression)expression.Expression;
                    var parentInstance = constantExpression.Value;
                    var instance = ((FieldInfo)expression.Member).GetValue(parentInstance);
                    return new[] { new NodeInfo(instance, propertyExpression.Member as PropertyInfo) };
                }

                throw new NotSupportedException("Cannot deal with expression");
            }).ToArray();
        }

        private NodeInfo GetNodeInfo<TProp>(Expression<Func<TProp>> target)
        {
            var propertyExpression = target.Body as MemberExpression;
            if (propertyExpression != null)
            {
                var expression = propertyExpression.Expression as MemberExpression;
                var constantExpression = (ConstantExpression)expression.Expression;
                var parentInstance = constantExpression.Value;
                var instance = ((FieldInfo)expression.Member).GetValue(parentInstance);
                return new NodeInfo(instance, propertyExpression.Member as PropertyInfo);
            }

            throw new NotSupportedException("Cannot deal with expression");
        }
    }
}