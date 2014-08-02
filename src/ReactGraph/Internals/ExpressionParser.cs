using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ReactGraph.Internals
{
    class ExpressionParser
    {
        readonly NodeRepository nodeRepository;

        public ExpressionParser(NodeRepository nodeRepository)
        {
            this.nodeRepository = nodeRepository;
        }

        public INodeInfo GetNodeInfo<TProp>(Expression<Func<TProp>> target)
        {
            return new GetNodeVisitor<TProp>(nodeRepository).GetNode(target);
        }

        class GetNodeVisitor<T> : ExpressionVisitor
        {
            readonly Stack<MemberExpression> path = new Stack<MemberExpression>();
            readonly NodeRepository nodeRepository;
            INodeInfo formulaNode;

            public GetNodeVisitor(NodeRepository nodeRepository)
            {
                this.nodeRepository = nodeRepository;
            }

            public INodeInfo GetNode(Expression target)
            {
                Visit(target);
                return formulaNode.ReduceIfPossible();
            }

            protected override Expression VisitLambda<T1>(Expression<T1> node)
            {
                formulaNode = nodeRepository.GetOrCreate<T>(node);
                return base.VisitLambda(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                path.Push(node);
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                var currentValue = node.Value;
                var rootValue = node.Value;
                INodeInfo currentNode = null;
                while (path.Count > 0)
                {
                    var expression = path.Pop();
                    var nodeInfo = nodeRepository.GetOrCreate(rootValue, currentValue, expression.Member, expression);
                    var nodeValue = ((IValueSource) nodeInfo).GetValue();
                    if (currentNode == null)
                    {
                        rootValue = nodeValue;
                        nodeInfo.RootInstance = rootValue;
                    }
                    else if (node.Value != currentNode.ParentInstance && !nodeInfo.Dependencies.Contains(currentNode))
                    {
                        nodeInfo.Dependencies.Add(currentNode);
                    }
                    currentNode = nodeInfo;
                    currentValue = nodeValue;
                }

                if (currentNode != null && !formulaNode.Dependencies.Contains(currentNode))
                    formulaNode.Dependencies.Add(currentNode);

                return base.VisitConstant(node);
            }
        }
    }
}