using System;

namespace ReactGraph.NodeInfo
{
    interface IWritableNodeInfo<T> : INodeInfo
    {
        void SetSource(IValueSource<T> formulaNode, Action<Exception> errorHandler);
    }
}