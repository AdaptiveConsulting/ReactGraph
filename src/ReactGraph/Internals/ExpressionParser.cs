using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Internals
{
    internal class ExpressionParser
    {
        public NodeInfo[] GetSourceVerticies<TProp>(Expression<Func<TProp>> formula)
        {
            return new GetNodeVisitor().GetNodes(formula);
        }

        public NodeInfo GetNodeInfo<TProp>(Expression<Func<TProp>> target, Expression<Func<TProp>> formula)
        {
            var getVal2 = formula.Compile();
            var visit = new GetNodeVisitor();
            return visit.GetNode(target, () => getVal2());
        }

        class GetNodeVisitor : ExpressionVisitor
        {
            readonly Stack<Func<object, object>> path = new Stack<Func<object, object>>();
            private PropertyInfo propertyInfo;
            private readonly List<NodeInfo> nodes = new List<NodeInfo>();
            private Func<object> val;

            public NodeInfo GetNode(Expression target, Func<object> getValue = null)
            {
                val = getValue;
                Visit(target);
                return nodes.Single();
            }

            public NodeInfo[] GetNodes(Expression formula)
            {
                Visit(formula);
                return nodes.ToArray();
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var property = node.Member as PropertyInfo;
                if (propertyInfo == null)
                    propertyInfo = property;
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
                var localInstance = node.Value;
                while (path.Count > 0)
                {
                    localInstance = path.Pop()(localInstance);
                }
                var localPropertyInfo = propertyInfo;
                var reevaluateValue = val == null ? 
                    (Action)null : 
                    () => localPropertyInfo.SetValue(localInstance, val(), null);
                nodes.Add(new NodeInfo(localInstance, localPropertyInfo, reevaluateValue));
                propertyInfo = null;
                return base.VisitConstant(node);
            }
        }
    }
}