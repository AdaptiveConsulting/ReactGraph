namespace ReactGraph.NodeInfo
{
    using System;

    class WriteOnlyNode<T> : ITakeValue<T>
    {
        readonly Action<Exception> exceptionHandler;
        readonly Action<T> setValue;
        IValueSource<T> valueSource;
        VisualisationInfo visualisationInfo;

        public WriteOnlyNode(Action<T> setValue, Action<Exception> exceptionHandler, string path)
        {
            visualisationInfo = new VisualisationInfo(NodeType.Action);
            this.setValue = setValue;
            this.FullPath = path;
            this.exceptionHandler = exceptionHandler;
        }

        public string FullPath { get; private set; }

        public void SetSource(IValueSource<T> sourceNode)
        {
            if (this.valueSource != null)
                throw new InvalidOperationException(string.Format("{0} already has a source associated with it", this.FullPath));

            this.valueSource = sourceNode;
        }

        public VisualisationInfo VisualisationInfo { get { return visualisationInfo; } }

        public ReevaluationResult Reevaluate()
        {
            // TODO again, I think the engine should do that
            if (this.valueSource != null)
            {
                this.UnderlyingValueHasBeenChanged();
                var value = this.valueSource.GetValue();
                if (value.HasValue)
                {
                    // TODO Don't set and return NoChange when value has not changed
                    this.setValue(value.Value);
                    return ReevaluationResult.Changed;
                }

                this.exceptionHandler(value.Exception);
                return ReevaluationResult.Error;
            }

            return ReevaluationResult.NoChange;
        }

        public void UnderlyingValueHasBeenChanged()
        {
        }

        public bool PathMatches(string pathToChangedValue)
        {
            return false;
        }

        public override string ToString()
        {
            return this.FullPath;
        }

        protected bool Equals(WriteOnlyNode<T> other)
        {
            return string.Equals(this.FullPath, other.FullPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((WriteOnlyNode<T>)obj);
        }

        public override int GetHashCode()
        {
            return this.FullPath.GetHashCode();
        }
    }
}