using System;
using System.Collections;
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
            var body = (MethodCallExpression)formula.Body;
            return body.Arguments.Select(a => new GetNodeVisitor().GetNode(a)).ToArray();
        }

        public NodeInfo GetNodeInfo<TProp>(Expression<Func<TProp>> target, Expression<Func<TProp>> formula)
        {
            var getVal2 = formula.Compile();
            var visit = new GetNodeVisitor();
            return visit.GetNode(target, () => getVal2());
        }

        class GetNodeVisitor : ExpressionVisitor
        {
            readonly Stack<Func<object, object>> _path = new Stack<Func<object, object>>();
            private PropertyInfo _propertyInfo;
            private object _instance;

            public NodeInfo GetNode(Expression target)
            {
                Visit(target);
                return new NodeInfo(_instance, _propertyInfo, null);
            }

            public NodeInfo GetNode(Expression target, Func<object> getValue)
            {
                Visit(target);
                return new NodeInfo(_instance, _propertyInfo, () => _propertyInfo.SetValue(_instance, getValue(), null));
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var property = node.Member as PropertyInfo;
                if (_propertyInfo == null)
                    _propertyInfo = property;
                else
                {
                    var fieldInfo = node.Member as FieldInfo;
                    if (property != null)
                    {
                        _path.Push(o => property.GetValue(o, null));
                    }
                    else if (fieldInfo != null)
                    {
                        _path.Push(fieldInfo.GetValue);
                    }
                }
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                var localInstance = node.Value;
                while (_path.Count > 0)
                {
                    localInstance = _path.Pop()(localInstance);
                }
                _instance = localInstance;
                return base.VisitConstant(node);
            }
        }
    }
}