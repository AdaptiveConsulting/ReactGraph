using System.Collections.Generic;

namespace ReactGraph
{
    class DefinitionComparer : IEqualityComparer<IDefinitionIdentity>
    {
        // TODO we really need to be sure that path is good enough for identity. Is that the case?

        public bool Equals(IDefinitionIdentity x, IDefinitionIdentity y)
        {
            if (ReferenceEquals(x, y)) return true;
            return string.Equals(x.FullPath, y.FullPath);
        }

        public int GetHashCode(IDefinitionIdentity obj)
        {
            unchecked
            {
                // TODO do we allow IDefinitionIdentity implementations to have a null path? We probably shouldn't?
                return obj.FullPath != null ? obj.FullPath.GetHashCode() : 0;
            }
        }
    }
}