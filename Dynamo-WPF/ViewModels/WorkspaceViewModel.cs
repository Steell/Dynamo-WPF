using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Dynamo.UI.Models;
using ReactiveUI;

namespace Dynamo.UI.Wpf.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IDisposable
    {
        private readonly List<IDisposable> subscriptions = new List<IDisposable>(); 
        
        public void Dispose()
        {
            foreach (var sub in subscriptions)
                sub.Dispose();
        }

        public void RegisterSubscriptionForDisposal(IDisposable subscription)
        {
            subscriptions.Add(subscription);
        }

        public void RegisterSubscriptionsForDisposal(params IDisposable[] subs)
        {
            subscriptions.AddRange(subs);
        }
    }

    public abstract class AWorkspaceViewModel : ViewModelBase
    {
        
    }

    public class WorkspaceViewModel : AWorkspaceViewModel
    {
        /* Properties */

        public WorkspaceViewModel(WorkspaceModel model)
        {
            var nodes = new ReactiveList<Node>();
            var connectors = new ReactiveList<Connector>();
            var notes = new ReactiveList<Note>();

            RegisterSubscriptionsForDisposal(
                model.NewNodeStream.Buffer().Subscribe(nodes.AddRange),
                model.DeletedNodeStream.Buffer().Subscribe(nodes.RemoveAll),
                model.NewConnectorStream.Buffer().Subscribe(connectors.AddRange),
                model.DeletedConnectorStream.Buffer().Subscribe(connectors.RemoveAll),
                model.NewNoteStream.Buffer().Subscribe(notes.AddRange),
                model.DeletedNoteStream.Buffer().Subscribe(notes.RemoveAll));

            WorkspaceElements = new CompositeCollection
            {
                nodes.CreateDerivedCollection(x => new NodeViewModel(x)),
                connectors.CreateDerivedCollection(x => new ConnectorViewModel(x)),
                notes.CreateDerivedCollection(x => new NoteViewModel(x))
            };

            Name = model.Filename;
        }

        public string Name { get; private set; }

        public CompositeCollection WorkspaceElements { get; private set; }

        // IsCurrentSpace -- Used to make the workspace hit test visible
        // WorkspaceElements -- All things that can be placed and moved in the workspace
        // Left/Top/ZIndex -- Canvas positioning
        // CanFindNodesFromElements -- Flag determining whether or not we can find nodes by selected geometry
        // IsHomeSpace -- Is this the home workspace?
        // IsPanning
        // IsOrbiting
        // CanNavigateBackground
        // FileName
        // HasUnsavedChanges

        /* Commands */

        // AlignSelected -- Aligns nodes using the given string parameter as a strategy
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
    }
}