using System;
using System.Collections.Generic;

namespace ReactGraph
{
    public interface ISourceDefinition : IDefinitionIdentity
    {
        List<ISourceDefinition> SourcePaths { get; }

        Type SourceType { get; }
    }

    public interface ISourceDefinition<T> : ISourceDefinition
    {
        Func<T, T> CreateGetValueDelegateWithCurrentValue();
        Func<T> CreateGetValueDelegate();
        string PathFromParent { get; }
    }
}