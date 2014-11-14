using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph
{
    public class AssignPropertyBuilder<T> : BuilderBase
    {
        readonly MemberDefinition<T> targetMemberDefinition;
        readonly DependencyEngine engine;

        public AssignPropertyBuilder(DependencyEngine engine, Expression<Func<T>> targetMemberExpression, string nodeId)
        {
            var memberExpression = targetMemberExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression must be a member (field/property) accessor, for example foo.Bar", "targetMemberExpression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            var fieldInfo = memberExpression.Member as FieldInfo;

            if (propertyInfo == null && fieldInfo == null)
                throw new ArgumentException("Only fields and properties are supported", "targetMemberExpression");
            if (propertyInfo != null && !propertyInfo.CanWrite)
                throw new ArgumentException("Property must be writable", "targetMemberExpression");
            if (fieldInfo != null && fieldInfo.IsInitOnly)
                throw new ArgumentException("Field cannot be read-only", "targetMemberExpression");

            this.engine = engine;
            targetMemberDefinition = CreateMemberDefinition(targetMemberExpression, nodeId, true, true);
        }

        public WhenFormulaChangesBuilder<T> From(Expression<Func<T>> sourceExpression, Action<Exception> onError, string nodeId = null)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var wrapped = Expression.Lambda<Func<T, T>>(sourceExpression.Body, new[] { parameterExpression });
            return From(wrapped, onError, nodeId);
        }

        public WhenFormulaChangesBuilder<T> From(Expression<Func<T, T>> sourceExpression, Action<Exception> onError, string nodeId = null)
        {
            ISourceDefinition<T> sourceDefinition;
            if (sourceExpression.IsWritable())
            {
                sourceDefinition = CreateMemberDefinition(sourceExpression, nodeId, true, true);
            }
            else
            {
                sourceDefinition = CreateFormulaDefinition(sourceExpression, nodeId, true);
            }

            engine.AddExpression(sourceDefinition, targetMemberDefinition, onError);
            return new WhenFormulaChangesBuilder<T>(sourceExpression, nodeId, engine);
        }
    }
}