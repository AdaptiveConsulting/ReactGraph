using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public class WhenFormulaChangesBuilder<T> : BuilderBase
    {
        readonly ISourceDefinition<T> sourceDefinition;
        readonly DependencyEngine dependencyEngine;

        public WhenFormulaChangesBuilder(Expression<Func<T>> sourceFunction, string nodeId, DependencyEngine dependencyEngine)
        {
            this.dependencyEngine = dependencyEngine;
            if (IsWritable(sourceFunction))
                sourceDefinition = CreateMemberDefinition(sourceFunction, nodeId, true);
            else
                sourceDefinition = CreateFormulaDefinition(sourceFunction, nodeId, true);
        }

        public void Do(Expression<Action<T>> action, Action<Exception> onError, string actionId = null)
        {
            dependencyEngine.AddExpression(sourceDefinition, new ActionDefinition<T>(action, actionId), onError);
        }
    }
}