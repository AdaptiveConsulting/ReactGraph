using System;
using System.Linq.Expressions;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class ActionDefinition<T> : ExpressionDefinition<T>, ITargetDefinition<T>
    {
        readonly Expression<Action<T>> expression;

        public ActionDefinition(Expression<Action<T>> expression, string nodeName)
            : base(expression, NodeType.Action, nodeName)
        {
            this.expression = expression;
        }

        public Action<T> CreateSetValueDelegate()
        {
            return expression.Compile();
        }
    }
}