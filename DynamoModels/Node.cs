using System;
using System.Reflection;

using ReactiveUI;

namespace Dynamo.UI.Models
{
    public class Node
    {
        public IReactiveCollection<PortModel> InPorts;
        public IReactiveCollection<PortModel> OutPorts;
        public IObservable<string> NickNameChanged;
        public IObservable<string> DescriptionChanged;
        public IObservable<LacingStrategy> LacingChanged;
    }

    public struct NodeInfo
    {
        public NodeInfo(MethodInfo method)
        {
        }

        public Node ToNode()
        {
            throw new NotImplementedException();
        }
    }
}
