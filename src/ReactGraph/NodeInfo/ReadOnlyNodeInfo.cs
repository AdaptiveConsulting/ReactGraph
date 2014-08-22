using System;

namespace ReactGraph.NodeInfo
{
    class ReadOnlyNodeInfo<T> : IValueSource<T>
    {
        readonly Maybe<T> currentValue = new Maybe<T>();
        readonly Func<T> getValue;

        public ReadOnlyNodeInfo(Func<T> getValue, string path)
        {
            Path = path;
            this.getValue = getValue;
            ValueChanged();
        }

        public Maybe<T> GetValue()
        {
            return currentValue;
        }

        public NodeType Type { get { return NodeType.Formula; } }

        public string Path { get; private set; }

        public ReevaluationResult Reevaluate()
        {
            ValueChanged();
            // Formulas do not report errors,
            // anything that relies on this formula will report the error
            return ReevaluationResult.Changed;
        }

        public void ValueChanged()
        {
            try
            {
                currentValue.NewValue(getValue());
            }
            catch (Exception ex)
            {
                currentValue.CouldNotCalculate(ex);
            }
        }

        IMaybe IValueSource.GetValue()
        {
            return GetValue();
        }

        public override string ToString()
        {
            return Path;
        }
    }
}