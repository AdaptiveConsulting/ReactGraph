using System;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Construction;

namespace ReactGraph
{
    public class BuilderBase
    {
        // TODO this needs to go somewhere else, some factory
        public static MemberDefinition<T> CreateMemberDefinition<T>(Expression<Func<T>> expression, string nodeId, bool calculateChildren, bool isWritable, string pathOverride = null)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var targetAssignmentLambda = isWritable ? Expression.Lambda<Action<T>>(Expression.Assign(expression.Body, parameterExpression), parameterExpression) : null;

            var memberDefinition = new MemberDefinition<T>(expression, targetAssignmentLambda, nodeId, isWritable, pathOverride);
            if (calculateChildren)
            {
                var sourceDefinitions = ExpressionParser.GetChildSources(expression);
                // Property expressions always return themselves as a child, so skip
                memberDefinition.SourcePaths.AddRange(sourceDefinitions.Single().SourcePaths);
            }
            return memberDefinition;
        }

        public static MemberDefinition<T> CreateMemberDefinition<T>(Expression<Func<T, T>> expression, string nodeId, bool calculateChildren, bool isWritable, string pathOverride = null)
        {
            var memberAccessor = Expression.Lambda<Func<T>>(expression.Body);
            return CreateMemberDefinition(memberAccessor, nodeId, calculateChildren, isWritable, pathOverride);
        }

        public static ISourceDefinition<T> CreateFormulaDefinition<T>(Expression<Func<T>> sourceExpression,
            string nodeId, bool calculateChildren, string pathOverride = null)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var wrapped = Expression.Lambda<Func<T, T>>(sourceExpression.Body, new[] {parameterExpression});
            return CreateFormulaDefinition(wrapped, nodeId, calculateChildren, pathOverride);
        }

        public static ISourceDefinition<T> CreateFormulaDefinition<T>(Expression<Func<T, T>> sourceExpression, string nodeId, bool calculateChildren, string pathOverride = null)
        {
            var formulaDefinition = new FormulaDefinition<T>(sourceExpression, nodeId, pathOverride);
            if (calculateChildren)
                formulaDefinition.SourcePaths.AddRange(ExpressionParser.GetChildSources(sourceExpression));

            return formulaDefinition;
        }
    }
}