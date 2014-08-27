using System;
using System.Collections.Generic;

namespace ReactGraph
{
    public interface ISourceDefinition : IDefinitionIdentity
    {
        List<ISourceDefinition> SourcePaths { get; }

        Type SourceType { get; }
    }

    public interface ISourceDefinition<out T> : ISourceDefinition
    {
        Func<T> CreateGetValueDelegate();
    }
}