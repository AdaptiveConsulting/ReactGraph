using System.Collections.Generic;

namespace ReactGraph
{
    class DefinitionComparer : IEqualityComparer<IDefinitionIdentity>
    {
        public bool Equals(IDefinitionIdentity x, IDefinitionIdentity y)
        {
            if (ReferenceEquals(x, y)) return true;
            return string.Equals(x.Path, y.Path);
        }

        public int GetHashCode(IDefinitionIdentity obj)
        {
            unchecked
            {
                return obj.Path != null ? obj.Path.GetHashCode() : 0;
            }
        }
    }
}