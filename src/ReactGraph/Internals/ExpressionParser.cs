using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Internals
{
    internal class ExpressionParser
    {
        private readonly Notifications notificationStrategies;

        public ExpressionParser(Notifications notificationStrategies)
        {
            this.notificationStrategies = notificationStrategies;
        }

        public FormulaExpressionInfo<T> GetFormulaExpressionInfo<T>(Expression<Func<T>> formula)
        {
            return new GetNodeVisitor<T>(notificationStrategies).GetFormulaNode(formula);
        }

        public PropertyNodeInfo<TProp> GetNodeInfo<TProp>(Expression<Func<TProp>> target)
        {
            var visit = new GetNodeVisitor<TProp>(notificationStrategies);
            return visit.GetNode(target);
        }

        class GetNodeVisitor<T> : ExpressionVisitor
        {
            readonly Stack<Func<object, object>> path = new Stack<Func<object, object>>();
            private PropertyInfo propertyInfo;
            private MemberExpression propertyExpression;
            private readonly List<PropertyNodeInfo<T>> nodes = new List<PropertyNodeInfo<T>>();
            private readonly Notifications notificationStrategies;

            public GetNodeVisitor(Notifications notificationStrategies)
            {
                this.notificationStrategies = notificationStrategies;
            }

            public PropertyNodeInfo<T> GetNode(Expression target)
            {
                Visit(target);
                return nodes.Single();
            }

            public FormulaExpressionInfo<T> GetFormulaNode(Expression<Func<T>> formula)
            {
                Visit(formula);
                return new FormulaExpressionInfo<T>(formula, nodes.ToArray());
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
                    nodes.Add(new PropertyNodeInfo<T>(rootValue, (T)localInstance, localPropertyInfo, localPropertyExpression, notificationStrategies));
                    propertyInfo = null;
                    propertyExpression = null;
                }
                
                return base.VisitConstant(node);
            }
        }
    }
}