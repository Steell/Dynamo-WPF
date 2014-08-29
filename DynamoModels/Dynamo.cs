using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Reactive;
using System.Reactive.Linq;

using ObservableExtensions;

using ReactiveUI;

namespace Dynamo.UI.Models
{
    public class Dynamo : IDisposable
    {
        private static IEnumerable<BigInteger> Tokens()
        {
            return Generate(BigInteger.Zero, x => x + 1);
        }

        private static IEnumerable<T> Generate<T>(T start, Func<T, T> generator)
        {
            while (true)
            {
                yield return start = generator(start);
            }
        }

        public Dynamo(
            DynamoState? initialState = null,
            IObservable<HomeWorkspaceState> homeWorkspaces = null,
            IObservable<CustomNodeWorkspaceState> customNodes = null,
            IObservable<IWorkspace> activeWorkspaces = null,
            IObservable<string> openWorkspaceFile = null,
            IObservable<string> importLibrary = null, 
            IObservable<bool> runAuto = null)
        {
            var init = initialState ?? DynamoState.Default;

            var homeWorkspaceStream = homeWorkspaces ?? Observable.Never<HomeWorkspaceState>();
            var customNodeStream = customNodes ?? Observable.Never<CustomNodeWorkspaceState>();
            var openFileStream = openWorkspaceFile ?? Observable.Never<string>();
            var importLibraryStream = importLibrary ?? Observable.Never<string>();
            var runAutoStream = (runAuto ?? Observable.Never<bool>()).StartWith(init.RunAuto);
            var activeWorkspaceTokens =
                (activeWorkspaces ?? Observable.Never<IWorkspace>()).Select(x => x.Token);

            var tokens = Tokens();
            IObservable<Node> newNodeFromLibraryStream = null; //TODO

            var createHomeWorkspace =
                new Func<HomeWorkspaceState, Func<BigInteger, IWorkspace>>(
                    state => 
                        token => 
                            new HomeWorkspace(
                                token, state, 
                                nodeCreatedStream:
                                    activeWorkspaceTokens.Select(
                                        activeToken =>
                                            newNodeFromLibraryStream.Where(_ => token == activeToken)).Switch()));

            var createCustomNode =
                new Func<CustomNodeWorkspaceState, Func<BigInteger, IWorkspace>>(
                    state => token => new CustomNodeWorkspace(token, state));
            
            //TODO: File loading as well
            var homeWorkspaceModels = homeWorkspaceStream.Select(createHomeWorkspace);
            var customNodeWorkspaceModels = customNodeStream.Select(createCustomNode);

            var workspaces = 
                Observable.Merge(homeWorkspaceModels, customNodeWorkspaceModels)
                    .Zip(tokens, (func, integer) => func(integer));
        }

        public IObservable<string> RecentFiles { get; private set; }
        public IObservable<LogEntry> ConsoleStream { get; private set; }
        
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public struct NodeState { }

    public struct DynamoState
    {
        public static readonly DynamoState Default = new DynamoState();

        public readonly bool RunAuto;

        public DynamoState(
            bool runAuto=false) 
            
            : this()
        {
            RunAuto = runAuto;
        }
    }
}
