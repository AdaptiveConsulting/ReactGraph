using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public class WhenFormulaChangesBuilder<T> : BuilderBase
    {
        readonly ISourceDefinition<T> sourceDefinition;
        readonly DependencyEngine dependencyEngine;

        public WhenFormulaChangesBuilder(ISourceDefinition<T> sourceDefinition, DependencyEngine dependencyEngine)
        {
            this.dependencyEngine = dependencyEngine;
            this.sourceDefinition = sourceDefinition;
        }

        public void Do(Expression<Action<T>> action, Action<Exception> onError, string actionId = null)
        {
            dependencyEngine.AddExpression(sourceDefinition, new ActionDefinition<T>(action, actionId), onError);
        }
    }
}