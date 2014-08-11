using System;
using System.Collections.Generic;
using ReactiveUI;

namespace Dynamo.UI.Models
{
    public class Dynamo
    {
        public Dynamo()
        {
        }

        public IObservable<string> RecentFiles { get; private set; }
    }
}
