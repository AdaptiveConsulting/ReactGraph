using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public interface IExpressionDefinition<T>
    {
        IMemberDefinition Bind(Expression<Func<T>> targetProperty);
        IExpressionDefinition<T> Metadata(string label = null, string color = null);
    }
}