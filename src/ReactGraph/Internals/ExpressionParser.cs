using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Internals
{
    internal class ExpressionParser
    {
        public DependencyInfo[] GetSourceVerticies(Expression formula)
        {
            return new GetNodeVisitor().GetNodes(formula);
        }

        public DependencyInfo GetNodeInfo<TProp>(Expression target, Expression<Func<TProp>> formula)
        {
            var getVal2 = formula.Compile();
            var visit = new GetNodeVisitor();
            return visit.GetNode(target, () => getVal2());
        }

        class GetNodeVisitor : ExpressionVisitor
        {
            readonly Stack<Func<object, object>> path = new Stack<Func<object, object>>();
            private PropertyInfo propertyInfo;
            private MemberExpression propertyExpression;
            private readonly List<DependencyInfo> nodes = new List<DependencyInfo>();
            private Func<object> val;

            public DependencyInfo GetNode(Expression target, Func<object> getValue = null)
            {
                val = getValue;
                Visit(target);
                return nodes.Single();
            }

            public DependencyInfo[] GetNodes(Expression formula)
            {
                Visit(formula);
                return nodes.ToArray();
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
                    var localPropertyInfo = propertyInfo;
                    var localPropertyExpression = propertyExpression;
                    var reevaluateValue = val == null ? (Action)null : () => localPropertyInfo.SetValue(localInstance, val(), null);
                    nodes.Add(new DependencyInfo(rootValue, localInstance, localPropertyInfo, localPropertyExpression, reevaluateValue));
                    propertyInfo = null;
                    propertyExpression = null;
                }
                
                return base.VisitConstant(node);
            }
        }
    }
}