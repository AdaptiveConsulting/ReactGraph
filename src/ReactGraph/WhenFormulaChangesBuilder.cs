using System;
using System.Linq.Expressions;
using ReactGraph.Construction;

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
                sourceDefinition = CreateMemberDefinition(sourceFunction, nodeId, true, ExpressionParser.GetRootOf(sourceFunction));
            else
                sourceDefinition = CreateFormulaDefinition(sourceFunction, nodeId, true, ExpressionParser.GetRootOf(sourceFunction));
        }

        public void Do(Expression<Action<T>> action, Action<Exception> onError, string actionId = null)
        {
            dependencyEngine.AddExpression(sourceDefinition, new ActionDefinition<T>(action, actionId, ExpressionParser.GetRootOf(action)), onError);
        }
    }
}