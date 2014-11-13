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
            static readonly MethodInfo CreateThisGenericMethod;

            static GetNodeVisitor()
            {
                ToPathGenericMethod = typeof(GetNodeVisitor).GetMethod("ToPath", BindingFlags.NonPublic | BindingFlags.Instance);
                CreateThisGenericMethod = typeof(GetNodeVisitor).GetMethod("CreateThis", BindingFlags.NonPublic | BindingFlags.Instance);
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
                    // Create node for 'this'
                    currentTopLevelDefinition.SourcePaths.Add(MemberToSourcePath(node));

                    subExpressions.Add(current);
                    current = null;
                    currentTopLevelDefinition = null;
                }
                return base.VisitConstant(node);
            }

            ISourceDefinition MemberToSourcePath(ConstantExpression node)
            {
                return (ISourceDefinition)CreateThisGenericMethod.MakeGenericMethod(node.Type).Invoke(this, new object[] { node });
            }

            ISourceDefinition MemberToSourcePath(MemberExpression node)
            {
                return (ISourceDefinition)ToPathGenericMethod.MakeGenericMethod(node.Type).Invoke(this, new object[] { node });
            }

            [UsedImplicitly]
            ISourceDefinition CreateThis<T>(ConstantExpression node)
            {
                var getter = Expression.Lambda<Func<T>>(node);
                return BuilderBase.CreateMemberDefinition(getter, null, false, false, pathOverride: "this");
            }

            [UsedImplicitly]
            ISourceDefinition ToPath<T>(MemberExpression node)
            {
                var getter = Expression.Lambda<Func<T>>(node);
                return BuilderBase.CreateMemberDefinition(getter, null, false, node.IsWritable());
            }
        }
    }
}