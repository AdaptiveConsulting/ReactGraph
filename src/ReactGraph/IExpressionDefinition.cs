using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public interface IExpressionDefinition
    {
        IMemberDefinition Bind<TProp>(Expression<Func<TProp>> targetProperty);
        IExpressionDefinition Metadata(string label = null, string color = null);
    }
}