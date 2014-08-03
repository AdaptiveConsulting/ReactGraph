using System;
using System.Linq.Expressions;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Construction
{
    class FormulaDescriptor<T> : DependencyDescriptor<T>
    {
        readonly Expression<Func<T>> node;
        readonly Func<T> executeNode;

        public FormulaDescriptor(Expression<Func<T>> node)
        {
            this.node = node;
            var expression = ((Expression<Func<T>>)new NullCheckRewriter().Visit(node));
            executeNode = expression.Compile();
        }

        public override INodeInfo GetOrCreateNodeInfo(NodeRepository repo)
        {
            return new FormulaExpressionInfo<T>(executeNode, ExpressionStringBuilder.ToString(node));
        }

        public override object GetValue()
        {
            return executeNode();
        }

        public override IWritableNodeInfo<T> GetOrCreateWritableNodeInfo(NodeRepository repo)
        {
            throw new NotSupportedException();
        }
    }
}