using System;

namespace ReactGraph.NodeInfo
{
    interface IValueSink<T>
    {
        void SetSource(IValueSource<T> formulaNode, Action<Exception> errorHandler);
    }
}