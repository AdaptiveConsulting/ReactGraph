using System;
using System.Linq.Expressions;
using ReactGraph.Construction;
using ReactGraph.NodeInfo;

namespace ReactGraph.Api
{
    class ActionNodeInfo<T> : IWritableNodeInfo<T>
    {
        readonly Action<T> action;
        readonly string label;
        Action<Exception> onError;
        IValueSource<T> formula;

        public ActionNodeInfo(Action<T> action, string nodeLabel)
        {
            this.action = action;
            label = nodeLabel;
        }

        public Maybe<T> GetValue()
        {
            throw new NotSupportedException();
        }

        IMaybe IValueSource.GetValue()
        {
            return GetValue();
        }

        public NodeType Type { get { return NodeType.Action;} }

        public ReevaluationResult Reevaluate()
        {
            var value = formula.GetValue();
            if (value.HasValue)
                action(value.Value);
            else
                onError(value.Exception);

            return ReevaluationResult.NoChange;
        }

        public void ValueChanged()
        {
        }

        public void UpdateSubscriptions(IMaybe newParent)
        {
        }

        public void SetSource(IValueSource<T> formulaNode, Action<Exception> errorHandler)
        {
            if (formula != null)
                throw new InvalidOperationException(string.Format("{0} already has a formula associated with it", label));

            formula = formulaNode;
            onError = errorHandler;
        }

        public override string ToString()
        {
            return label;
        }
    }
}