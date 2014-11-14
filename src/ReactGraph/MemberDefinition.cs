using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class MemberDefinition<T> : ExpressionDefinition, ISourceDefinition<T>, ITargetDefinition<T>
    {
        readonly Expression<Func<T>> targetMemberExpression;
        readonly Expression<Action<T>> assignmentLambda;

        public MemberDefinition(Expression<Func<T>> targetMemberExpression, Expression<Action<T>> assignmentLambda, string targetMemberId, bool isWritable, string pathOverride = null) :
            base(targetMemberExpression, NodeType.Member, targetMemberId, pathOverride)
        {
            this.targetMemberExpression = targetMemberExpression;
            this.assignmentLambda = assignmentLambda;
            PathFromParent = targetMemberExpression.Body is MemberExpression ? ((MemberExpression)targetMemberExpression.Body).Member.Name : null;
            SourcePaths = new List<ISourceDefinition>();
            IsWritable = isWritable;
        }

        public Func<T, T> CreateGetValueDelegateWithCurrentValue()
        {
            throw new NotSupportedException("Members should not be re-evaluated with a current value.");
        }

        public Func<T> CreateGetValueDelegate()
        {
            return targetMemberExpression.Compile();
        }

        public string PathFromParent { get; private set; }

        public Action<T> CreateSetValueDelegate()
        {
            return assignmentLambda.Compile();
        }

        public List<ISourceDefinition> SourcePaths { get; private set; }

        public Type SourceType { get { return typeof (T); } }

        public bool IsWritable { get; private set; }

        public override string ToString()
        {
            return FullPath;
        }
    }
}