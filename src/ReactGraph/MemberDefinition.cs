using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class MemberDefinition<T> : ExpressionDefinition<T>, ISourceDefinition<T>, ITargetDefinition<T>
    {
        readonly Expression<Func<T>> targetMemberExpression;
        readonly Expression<Action<T>> assignmentLambda;

        public MemberDefinition(Expression<Func<T>> targetMemberExpression, Expression<Action<T>> assignmentLambda, string targetMemberId) : 
            base(targetMemberExpression, NodeType.Member, targetMemberId)
        {
            this.targetMemberExpression = targetMemberExpression;
            this.assignmentLambda = assignmentLambda;
            SourcePaths = new List<ISourceDefinition>();
        }

        public Func<T> CreateGetValueDelegate()
        {
            return targetMemberExpression.Compile();
        }

        public Action<T> CreateSetValueDelegate()
        {
            return assignmentLambda.Compile();
        }

        public List<ISourceDefinition> SourcePaths { get; private set; }

        public Type SourceType { get { return typeof (T); } }
    }
}