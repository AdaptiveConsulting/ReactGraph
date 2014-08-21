using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Construction
{
    public static class ExpressionParser
    {
        public static FormulaDescriptor<TProp> GetFormulaDescriptor<TProp>(Expression<Func<TProp>> target)
        {
            return new GetNodeVisitor<TProp>().GetFormulaInfo(target);
        }

        public static SourceDescriptor<TProp> GetMemberDescriptor<TProp>(Expression<Func<TProp>> target)
        {
            return new GetNodeVisitor<TProp>().GetTargetInfo(target);
        }

        class GetNodeVisitor<T> : ExpressionVisitor
        {
            readonly Stack<MemberExpression> path = new Stack<MemberExpression>();
            FormulaDescriptor<T> formulaFormula;
            SourceDescriptor<T> sourceInfo;

            public FormulaDescriptor<T> GetFormulaInfo(Expression<Func<T>> target)
            {
                formulaFormula = new FormulaDescriptor<T>(target);
                Visit(target);
                return formulaFormula;
            }

            public SourceDescriptor<T> GetTargetInfo(Expression<Func<T>> target)
            {
                Visit(target);
                return sourceInfo;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                path.Push(node);
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                var parentValue = node.Value;
                var rootValue = node.Value;
                ExpressionDescriptor currentNode = null;
                while (path.Count > 0)
                {
                    var expression = path.Pop();
                    var nodeInfo = CreateMember(rootValue, parentValue, expression.Member, expression);
                    var nodeValue = nodeInfo.GetValue();
                    if (currentNode == null)
                    {
                        rootValue = nodeValue;
                        nodeInfo.RootInstance = rootValue;
                    }
                    else if (node.Value != currentNode.ParentInstance && !nodeInfo.SubExpressions.Contains(currentNode))
                    {
                        nodeInfo.SubExpressions.Add(currentNode);
                    }
                    currentNode = nodeInfo;
                    parentValue = nodeValue;
                }

                if (currentNode != null)
                {
                    if (formulaFormula == null)
                    {
                        if (sourceInfo != null)
                            throw new InvalidOperationException("Expression contains more than one property");
                        sourceInfo = (SourceDescriptor<T>) currentNode;
                    }
                    else if (!formulaFormula.SubExpressions.Contains(currentNode))
                        formulaFormula.SubExpressions.Add(currentNode);
                }

                return base.VisitConstant(node);
            }

            ExpressionDescriptor CreateMember(object rootValue, object parentInstance, MemberInfo member, MemberExpression expression)
            {
                var propertyInfo = member as PropertyInfo;
                ExpressionDescriptor expressionDescriptor;
                if (propertyInfo != null)
                {
                    var type = typeof(MemberSourceDescriptor<>).MakeGenericType(propertyInfo.PropertyType);
                    expressionDescriptor = (ExpressionDescriptor)Activator.CreateInstance(
                        type, rootValue, parentInstance,
                        propertyInfo, expression);
                }
                else
                {
                    var fieldInfo = ((FieldInfo)member);
                    var type = typeof(MemberSourceDescriptor<>).MakeGenericType(fieldInfo.FieldType);
                    expressionDescriptor = (ExpressionDescriptor)Activator.CreateInstance(
                        type, rootValue, parentInstance,
                        fieldInfo, expression);
                }

                return expressionDescriptor;
            }
        }
    }
}