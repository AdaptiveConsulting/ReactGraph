using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public interface IExpressionDefinition<T>
    {
        IMemberDefinition Bind<TProp>(Expression<Func<TProp>> targetProperty, Action<Exception> onError, string propertyId = null);

        IMemberDefinition Action(Expression<Action<T>> action, Action<Exception> onError, string actionId = null);

        IMemberDefinition Action(Action<T> action, string nodeLabel, Action<Exception> onError, string actionId = null);
    }
}