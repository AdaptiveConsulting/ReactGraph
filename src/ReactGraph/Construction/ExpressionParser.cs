using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ReactGraph.Properties;

namespace ReactGraph.Construction
{
    public static class ExpressionParser
    {
        public static object GetRootOf(Expression expression)
        {
            return new GetRootVisitor().GetRoot(expression);
        }

        public static List<ISourceDefinition> GetChildSources(Expression expr, object root)
        {
            return new GetNodeVisitor().GetSubExpressions(expr, root);
        }

        class GetRootVisitor : ExpressionVisitor
        {
            object root;
            MemberExpression lastMember;

            public object GetRoot(Expression expression)
            {
                Visit(expression);
                return root;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                lastMember = node;
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (lastMember == null)
                    root = node.Value;
                else
                {
                    var info = lastMember.Member as FieldInfo;
                    if (info != null)
                        root = info.GetValue(node.Value);
                    else
                    {
                        var member = lastMember.Member as PropertyInfo;
                        if (member != null)
                            root = member.GetValue(node.Value, null);
                    }
                    lastMember = null;
                }

                return base.VisitConstant(node);
            }
        }

        class GetNodeVisitor : ExpressionVisitor
        {
            static readonly MethodInfo ToPathGenericMethod;

            static GetNodeVisitor()
            {
                ToPathGenericMethod = typeof(GetNodeVisitor).GetMethod("ToPath", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            readonly List<ISourceDefinition> subExpressions = new List<ISourceDefinition>();
            ISourceDefinition currentTopLevelDefinition;
            ISourceDefinition current;
            object root;

            public List<ISourceDefinition> GetSubExpressions(Expression target, object root)
            {
                this.root = root;
                Visit(target);
                return subExpressions;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var sourcePath = MemberToSourcePath(node);
                if (currentTopLevelDefinition == null)
                {
                    currentTopLevelDefinition = sourcePath;
                    current = sourcePath;
                }
                else
                {
                    currentTopLevelDefinition.SourcePaths.Add(sourcePath);
                    currentTopLevelDefinition = sourcePath;
                }
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (current != null)
                {
                    subExpressions.Add(current);
                    current = null;
                    currentTopLevelDefinition = null;
                }
                return base.VisitConstant(node);
            }

            ISourceDefinition MemberToSourcePath(MemberExpression node)
            {
                return (ISourceDefinition) ToPathGenericMethod.MakeGenericMethod(node.Type).Invoke(this, new object[] {node});
            }

            [UsedImplicitly]
            ISourceDefinition ToPath<T>(MemberExpression node)
            {
                var getter = Expression.Lambda<Func<T>>(node);
                return BuilderBase.CreateMemberDefinition(getter, null, false, root);
            }
        }
    }
}