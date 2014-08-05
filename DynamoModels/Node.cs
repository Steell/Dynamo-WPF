using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
