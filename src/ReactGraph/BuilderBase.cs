using System;
using System.Linq.Expressions;
using System.Reflection;
using ReactGraph.Construction;

namespace ReactGraph
{
    public class BuilderBase
    {
        protected bool IsWritable<T>(Expression<Func<T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null) return false;
            var propertyInfo = memberExpression.Member as PropertyInfo;
            var fieldInfo = memberExpression.Member as FieldInfo;

            if (propertyInfo == null && fieldInfo == null) return false;
            if (propertyInfo != null && !propertyInfo.CanWrite) return false;
            if (fieldInfo != null && fieldInfo.IsInitOnly) return false;

            return true;
        }

        // TODO this needs to go somewhere else, some factory
        public static MemberDefinition<T> CreateMemberDefinition<T>(Expression<Func<T>> expression, string nodeId, bool calculateChildren)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var targetAssignmentLambda = Expression.Lambda<Action<T>>(Expression.Assign(expression.Body, parameterExpression), parameterExpression);

            var memberDefinition = new MemberDefinition<T>(expression, targetAssignmentLambda, nodeId);
            if (calculateChildren)
                memberDefinition.SourcePaths.AddRange(ExpressionParser.GetChildSources(expression, memberDefinition.Root));
            return memberDefinition;
        }

        public static ISourceDefinition<T> CreateFormulaDefinition<T>(Expression<Func<T>> sourceExpression, string nodeId, bool calculateChildren)
        {
            var formulaDefinition = new FormulaDefinition<T>(sourceExpression, nodeId);
            if (calculateChildren)
                formulaDefinition.SourcePaths.AddRange(ExpressionParser.GetChildSources(sourceExpression, formulaDefinition.Root));

            return formulaDefinition;
        }
    }
}