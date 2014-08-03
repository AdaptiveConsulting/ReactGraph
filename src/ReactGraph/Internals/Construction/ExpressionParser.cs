using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph.Internals.Construction
{
    class ExpressionParser
    {
        public FormulaDescriptor<TProp> GetFormulaInfo<TProp>(Expression<Func<TProp>> target)
        {
            return new GetNodeVisitor<TProp>().GetFormulaInfo(target);
        }

        public DependencyDescriptor<TProp> GetTargetInfo<TProp>(Expression<Func<TProp>> target)
        {
            return new GetNodeVisitor<TProp>().GetTargetInfo(target);
        }

        class GetNodeVisitor<T> : ExpressionVisitor
        {
            readonly Stack<MemberExpression> path = new Stack<MemberExpression>();
            FormulaDescriptor<T> formulaFormula;
            DependencyDescriptor<T> dependencyInfo;

            public FormulaDescriptor<T> GetFormulaInfo(Expression<Func<T>> target)
            {
                formulaFormula = new FormulaDescriptor<T>(target);
                Visit(target);
                return formulaFormula;
            }

            public DependencyDescriptor<T> GetTargetInfo(Expression<Func<T>> target)
            {
                Visit(target);
                return dependencyInfo;
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
                DependencyDescriptor currentNode = null;
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
                    else if (node.Value != currentNode.ParentInstance && !nodeInfo.Dependencies.Contains(currentNode))
                    {
                        nodeInfo.Dependencies.Add(currentNode);
                    }
                    currentNode = nodeInfo;
                    parentValue = nodeValue;
                }

                if (currentNode != null)
                {
                    if (formulaFormula == null)
                    {
                        if (dependencyInfo != null)
                            throw new InvalidOperationException("Expression contains more than one property");
                        dependencyInfo = (DependencyDescriptor<T>) currentNode;
                    }
                    else if (!formulaFormula.Dependencies.Contains(currentNode))
                        formulaFormula.Dependencies.Add(currentNode);
                }

                return base.VisitConstant(node);
            }

            DependencyDescriptor CreateMember(object rootValue, object parentInstance, MemberInfo member, MemberExpression expression)
            {
                var propertyInfo = member as PropertyInfo;
                DependencyDescriptor dependencyDescriptor;
                if (propertyInfo != null)
                {
                    var type = typeof(MemberDependencyDescriptor<>).MakeGenericType(propertyInfo.PropertyType);
                    dependencyDescriptor = (DependencyDescriptor)Activator.CreateInstance(
                        type, rootValue, parentInstance,
                        propertyInfo, expression);
                }
                else
                {
                    var fieldInfo = ((FieldInfo)member);
                    var type = typeof(MemberDependencyDescriptor<>).MakeGenericType(fieldInfo.FieldType);
                    dependencyDescriptor = (DependencyDescriptor)Activator.CreateInstance(
                        type, rootValue, parentInstance,
                        fieldInfo, expression);
                }

                return dependencyDescriptor;
            }
        }
    }
}