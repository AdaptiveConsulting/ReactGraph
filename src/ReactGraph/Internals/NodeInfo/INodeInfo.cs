using System.Collections.Generic;

namespace ReactGraph.Internals.NodeInfo
{
    interface INodeInfo
    {
        // runtime only
        void Reevaluate();

        // runtime only
        void ValueChanged();

        /// <summary>
        /// RootInstance is the instance which starts the expression, for example
        /// () => viewModel.Prop.AnotherProp would have a root instance of viewMode which never changes
        /// </summary>
        object RootInstance { get; set; } // runtime

        object ParentInstance { get; set; } // runtime

        List<INodeInfo> Dependencies { get; } // TODO shouldn't we use graph instead?

        INodeInfo ReduceIfPossible(); // construction only
    }
}