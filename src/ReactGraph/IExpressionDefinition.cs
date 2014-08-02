using System;
using System.Linq.Expressions;

namespace ReactGraph
{
    public interface IExpressionDefinition
    {
        void Bind<TProp>(Expression<Func<TProp>> targetProperty);
    }
}