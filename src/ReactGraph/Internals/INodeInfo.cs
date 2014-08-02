using System.Collections.Generic;

namespace ReactGraph.Internals
{
    interface INodeInfo
    {
        void Reevaluate();

        void ValueChanged();

        /// <summary>
        /// RootInstance is the instance which starts the expression, for example
        /// () => viewModel.Prop.AnotherProp would have a root instance of viewMode which never changes
        /// </summary>
        object RootInstance { get; }

        object ParentInstance { get; set; }

        string Key { get; }

        List<INodeInfo> Dependencies { get; }
    }
}