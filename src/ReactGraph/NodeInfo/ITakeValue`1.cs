using System;

namespace ReactGraph.NodeInfo
{
    interface ITakeValue<T> : INodeInfo
    {
        void SetSource(IValueSource<T> formulaNode, Action<Exception> errorHandler);
    }
}