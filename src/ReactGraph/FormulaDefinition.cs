using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class FormulaDefinition<T> : ExpressionDefinition<T>, ISourceDefinition<T>
    {
        readonly Expression<Func<T, T>> sourceExpression;

        public FormulaDefinition(Expression<Func<T, T>> sourceExpression, string nodeId) : 
            base(sourceExpression, NodeType.Formula, nodeId)
        {
            this.sourceExpression = sourceExpression;
            SourcePaths = new List<ISourceDefinition>();
        }

        public Func<T, T> CreateGetValueDelegate()
        {
            return sourceExpression.Compile();
        }

        public List<ISourceDefinition> SourcePaths { get; private set; }

        public Type SourceType { get { return typeof (T); } }
    }
}