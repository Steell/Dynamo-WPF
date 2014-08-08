using System;
using System.Reactive.Linq;
using System.Windows.Data;

using Dynamo.UI.Models;

using ObservableExtensions;
using ReactiveUI;

namespace Dynamo.UI.Wpf.ViewModels
{
    public abstract class AWorkspaceViewModel : ViewModelBase
    {
        
    }

    public class WorkspaceViewModel : AWorkspaceViewModel
    {
        /* Properties */

        public WorkspaceViewModel(Workspace model)
        {
            var nodes = new ReactiveList<Node>();
            var connectors = new ReactiveList<Connector<Node, int>>();
            var notes = new ReactiveList<Note>();

            var nodeVMs = nodes.CreateDerivedCollection(x => new NodeViewModel(x));
            var connectorVMs = connectors.CreateDerivedCollection(x => new ConnectorViewModel(x));
            var noteVMs = notes.CreateDerivedCollection(x => new NoteViewModel(x));

            var selection = new ReactiveList<object>();
            Selection = selection;

            var newNodeCommand = ReactiveCommand.Create();
            NewNodeCommand = newNodeCommand;
            var newNodeStream = newNodeCommand.Cast<NodeViewModel>().Select(x => x.Model);

            var deleteNodeCommand = ReactiveCommand.Create();
            DeleteNodeCommand = deleteNodeCommand;
            var deleteNodeStream = deleteNodeCommand.Cast<NodeViewModel>().Select(x => x.Model);

            RegisterSubscriptionsForDisposal(
                newNodeStream                 .Buffer() .Subscribe( nodes.AddRange       ),
                deleteNodeStream              .Buffer() .Subscribe( nodes.RemoveAll      ),
                model .ConnectorCreatedStream .Buffer() .Subscribe( connectors.AddRange  ),
                model .ConnectorDeletedStream .Buffer() .Subscribe( connectors.RemoveAll ));
                //model .NewNoteStream          .Buffer() .Subscribe( notes.AddRange       ),
                //model .DeletedNoteStream      .Buffer() .Subscribe( notes.RemoveAll      ));
                //nodeVMs.ItemsAdded.SelectMany( 
                //    node => node.NodeSelectedChanged.Select(selected => new { node, selected }))
                //    .Subscribe(selection.Add));

            WorkspaceElements = new CompositeCollection { nodeVMs, connectorVMs, noteVMs };
            Name = "NODE";  //TODO model.Filename;
        }

        public IReactiveCommand NewNodeCommand { get; private set; }

        public string Name { get; private set; }

        public CompositeCollection WorkspaceElements { get; private set; }

        public IReactiveCollection<object> Selection { get; private set; }

        public IReactiveCommand NewNoteCommand { get; private set; }
        public IReactiveCommand AutoLayoutCommand { get; private set; }
        public IReactiveCommand SelectNeighborsCommand { get; private set; }
        public IReactiveCommand DeleteSelectionCommand { get; private set; }
        public IReactiveCommand SelectAllCommand { get; private set; }

        // CanFindNodesFromElements -- Flag determining whether or not we can find nodes by selected geometry
        // IsHomeSpace -- Is this the home workspace?
        // IsPanning
        // IsOrbiting
        // CanNavigateBackground
        // FileName
        // HasUnsavedChanges

        /* Commands */

        // AlignSelected -- Aligns nodes using the given parameter as a strategy
        // NodeFromSelection -- Node collapsing
        // FindNodesFromSelection -- Find nodes from selected geometry
        // DynamoViewModel.PublishCurrentWorkspaceCommand -- Publishes current custom node
        // ZoomIn
        // ZoomOut
        // FitView
        // TogglePan
        // ToggleOrbit


        // TODO: Make a state enum, use converter to System.Windows.Input.Cursor
        // CurrentCursor -- Cursor used for ZoomBorder
        // IsCursorForced -- ForceCursor used for ZoomBorder

        // TODO: Make a lens class that allows for different views of the same Workspace
        // Left/Top/ZIndex -- Canvas positioning

        #region Old Commands and Properties

        // IsCurrentSpace -- Used to make the workspace hit test visible

        #endregion
    }
}