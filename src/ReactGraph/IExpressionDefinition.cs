using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public interface IExpressionDefinition<T>
    {
        IMemberDefinition Bind<TProp>(Expression<Func<TProp>> targetProperty, Action<Exception> onError);
        IExpressionDefinition<T> Metadata(string label = null, string color = null);
    }
}