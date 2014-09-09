using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public static class RegistrationExtensions
    {
        public static WhenFormulaChangesBuilder<TProp> When<TProp>(this DependencyEngine engine, Expression<Func<TProp>> sourceFunction, string expressionId = null)
        {
            var parameterExpression = Expression.Parameter(typeof(TProp));
            var wrapped = Expression.Lambda<Func<TProp, TProp>>(sourceFunction.Body, new[] { parameterExpression });
            return new WhenFormulaChangesBuilder<TProp>(sourceFunction.IsWritable() ? 
                BuilderBase.CreateMemberDefinition(sourceFunction, expressionId, true) : 
                BuilderBase.CreateFormulaDefinition(sourceFunction, expressionId, true), engine);
        }

        public static AssignPropertyBuilder<TProp> Assign<TProp>(this DependencyEngine engine, Expression<Func<TProp>> targetMemberExpression, string expressionId = null)
        {
            return new AssignPropertyBuilder<TProp>(engine, targetMemberExpression, expressionId);
        }
    }
}