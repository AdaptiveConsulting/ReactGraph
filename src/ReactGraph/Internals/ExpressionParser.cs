using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            INodeInfo currentLevelNodeInfo;

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
                formulaNode = currentLevelNodeInfo = nodeRepository.GetOrCreate<T>(node);
                return base.VisitLambda(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                path.Push(node);
                //var property = node.Member as PropertyInfo;
                //if (propertyExpression == null)
                //{
                //    propertyInfo = property;
                //    propertyExpression = node;
                //}
                //else
                //{
                //    var fieldInfo = node.Member as FieldInfo;
                //    if (property != null)
                //    {
                //    }
                //    else if (fieldInfo != null)
                //    {
                //        path.Push(fieldInfo.GetValue);
                //    }
                //}
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                var currentValue = node.Value;
                var rootValue = node.Value;
                INodeInfo rootNode = null;
                INodeInfo currentNode = null;
                while (path.Count > 0)
                {
                    var expression = path.Pop();
                    var nodeInfo = nodeRepository.GetOrCreate<T>(rootValue, currentValue, expression.Member, expression);
                    if (currentNode != null)
                        currentNode.Dependencies.Add(nodeInfo);
                    else
                        rootNode = nodeInfo;
                    currentNode = nodeInfo;
                    currentValue = ((IValueSource)nodeInfo).GetValue();
                }
                
                currentLevelNodeInfo.Dependencies.Add(rootNode.Dependencies.Single());

                return base.VisitConstant(node);
            }
        }
    }
}