using System;
using System.Collections.Generic;
using System.Linq;
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

            public object GetRoot(Expression expression)
            {
                Visit(expression);
                return root;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                root = node.Value;
                return base.VisitConstant(node);
            }
        }

        class GetNodeVisitor : ExpressionVisitor
        {
            static readonly MethodInfo ToPathGenericMethod;

            static GetNodeVisitor()
            {
                ToPathGenericMethod = typeof(GetNodeVisitor).GetMethod("ToPath", BindingFlags.NonPublic | BindingFlags.Static);
            }

            readonly List<ISourceDefinition> subExpressions = new List<ISourceDefinition>();
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
                if (current == null)
                {
                    current = sourcePath;
                }
                else
                {
                    current.SourcePaths.Add(sourcePath);
                    current = sourcePath;
                }
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (current != null)
                {
                    subExpressions.Add(current);
                    current = null;
                }
                return base.VisitConstant(node);
            }

            ISourceDefinition MemberToSourcePath(MemberExpression node)
            {
                return (ISourceDefinition) ToPathGenericMethod.MakeGenericMethod(node.Type).Invoke(null, new object[] {node});
            }

            [UsedImplicitly]
            static ISourceDefinition ToPath<T>(MemberExpression node)
            {
                var getter = Expression.Lambda<Func<T>>(node);
                return BuilderBase.CreateMemberDefinition(getter, null, false);
            }
        }
    }
}