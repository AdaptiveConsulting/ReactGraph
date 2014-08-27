using System.Collections.Generic;

namespace ReactGraph.Construction
{
    public abstract class ExpressionDescriptor
    {
        protected ExpressionDescriptor()
        {
            SubExpressions = new List<ExpressionDescriptor>();
        }

        public string Id { get; set; }

        public object RootInstance { get; set; }

        public object ParentInstance { get; protected set; }

        public List<ExpressionDescriptor> SubExpressions { get; private set; }
    }
}
