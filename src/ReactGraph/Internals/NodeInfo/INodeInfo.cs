using System.Collections.Generic;

namespace ReactGraph.Internals.NodeInfo
{
    interface INodeInfo
    {
        void Reevaluate();

        void ValueChanged();

        /// <summary>
        /// RootInstance is the instance which starts the expression, for example
        /// () => viewModel.Prop.AnotherProp would have a root instance of viewMode which never changes
        /// </summary>
        object RootInstance { get; set; }

        object ParentInstance { get; set; }

        List<INodeInfo> Dependencies { get; }

        INodeInfo ReduceIfPossible();
    }
}