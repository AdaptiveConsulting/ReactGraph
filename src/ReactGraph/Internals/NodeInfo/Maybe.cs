namespace ReactGraph.Internals.NodeInfo
{
    class Maybe<T> : IMaybe
    {
        public void NewValue(T value)
        {
            Value = value;
            HasValue = true;
        }

        public void ValueMissing()
        {
            Value = default(T);
            HasValue = false;
        }

        public bool HasValue { get; private set; }

        object IMaybe.Value { get { return Value; } }

        public T Value { get; private set; }
    }
}