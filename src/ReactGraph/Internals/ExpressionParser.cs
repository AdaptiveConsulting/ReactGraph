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
            readonly Stack<Func<object, object>> path = new Stack<Func<object, object>>();
            readonly List<INodeInfo> nodes = new List<INodeInfo>();
            readonly NodeRepository nodeRepository;
            MemberExpression propertyExpression;
            PropertyInfo propertyInfo;

            public GetNodeVisitor(NodeRepository nodeRepository)
            {
                this.nodeRepository = nodeRepository;
            }

            public INodeInfo GetNode(Expression target)
            {
                Visit(target);
                return nodes.Single();
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return base.VisitMethodCall(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var property = node.Member as PropertyInfo;
                if (propertyInfo == null)
                {
                    propertyInfo = property;
                    propertyExpression = node;
                }
                else
                {
                    var fieldInfo = node.Member as FieldInfo;
                    if (property != null)
                    {
                        path.Push(o => property.GetValue(o, null));
                    }
                    else if (fieldInfo != null)
                    {
                        path.Push(fieldInfo.GetValue);
                    }
                }
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (propertyInfo != null)
                {
                    var localInstance = node.Value;
                    var rootValueResolver = path.Count > 0 ? path.Peek() : null;
                    var rootValue = rootValueResolver == null ? node.Value : rootValueResolver(node.Value);
                    while (path.Count > 0)
                    {
                        localInstance = path.Pop()(localInstance);
                    }

                    var propertyNodeInfo = nodeRepository.GetOrCreate<T>(rootValue, localInstance, propertyInfo, propertyExpression);

                    nodes.Add(propertyNodeInfo);
                    propertyInfo = null;
                    propertyExpression = null;
                }
                
                return base.VisitConstant(node);
            }
        }
    }
}