using System;
using System.Linq.Expressions;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Construction
{
    class FormulaDescriptor<T> : DependencyDescriptor<T>
    {
        readonly Expression<Func<T>> node;
        readonly Func<T> executeNode;
        readonly string key;

        public FormulaDescriptor(Expression<Func<T>> node)
        {
            this.node = node;
            executeNode = node.Compile();
            key = this.node.ToString();
        }

        public override INodeInfo GetOrCreateNodeInfo(NodeRepository repo)
        {
            return new FormulaExpressionInfo<T>(executeNode, ExpressionStringBuilder.ToString(node));
        }

        public override object GetValue()
        {
            return executeNode();
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override IWritableNodeInfo<T> GetOrCreateWritableNodeInfo(NodeRepository repo)
        {
            throw new NotSupportedException();
        }

        public override string Key
        {
            get { return key; }
        }
    }
}