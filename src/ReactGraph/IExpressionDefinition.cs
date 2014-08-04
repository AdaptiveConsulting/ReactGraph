using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public interface IExpressionDefinition<T>
    {
        IMemberDefinition Bind<TProp>(Expression<Func<TProp>> targetProperty, Action<Exception> onError, string propertyId = null);
    }
}