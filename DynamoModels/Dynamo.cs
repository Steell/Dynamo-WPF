using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;

namespace Dynamo.UI.Models
{
    public class Dynamo
    {
        public Dynamo()
        {
        }

        public Dynamo(
            IObservable<NewCustomNodeEventArgs> customNodes, 
            IObservable<NewHomeWorkspaceEventArgs> homeWorkspaces,
            IObservable<Unit> displayStartPage,
            IObservable<string> openWorkspace,
            IObservable<string> importLibrary,
            IObservable<bool> runAuto)
        {
            throw new NotImplementedException();
        }

        public IObservable<string> RecentFiles { get; private set; }
    }
}
