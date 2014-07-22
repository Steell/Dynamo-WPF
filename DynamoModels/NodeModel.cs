using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Dynamo.UI.Models
{
    public class NodeModel
    {
        public IReactiveCollection<PortModel> InPorts;
        public IReactiveCollection<PortModel> OutPorts;
        public IObservable<string> NickNameChanged;
        public IObservable<string> DescriptionChanged;
        public IObservable<LacingStrategy> LacingChanged;
    }
}
