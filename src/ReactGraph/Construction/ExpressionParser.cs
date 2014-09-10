using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ReactGraph.Properties;

namespace ReactGraph.Construction
{
    public static class ExpressionParser
    {
        public static List<ISourceDefinition> GetChildSources(Expression expr)
        {
            return new GetNodeVisitor().GetSubExpressions(expr);
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

            public List<ISourceDefinition> GetSubExpressions(Expression target)
            {
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
                if (node.IsWritable())
                    return BuilderBase.CreateMemberDefinition(getter, null, false);

                var parameterExpression = Expression.Parameter(typeof(T));
                var wrapped = Expression.Lambda<Func<T, T>>(node, new[] { parameterExpression });
                return BuilderBase.CreateFormulaDefinition(wrapped, null, false);
            }
        }
    }
}