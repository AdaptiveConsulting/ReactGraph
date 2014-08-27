using System;

namespace ReactGraph
{
    public interface ITargetDefinition<in T> : IDefinitionIdentity
    {
        Action<T> CreateSetValueDelegate();
    }
}