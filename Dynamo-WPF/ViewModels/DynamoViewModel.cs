using System;
using System.Reactive.Linq;
using System.Windows.Input;

using Dynamo.UI.Models;

using ReactiveUI;

namespace Dynamo.UI.Wpf.ViewModels
{
    public class DynamoViewModel : ViewModelBase
    {
        public DynamoViewModel()
        {
            #region Active Workspace Operations
            var currentWorkspaceStream =
                this.ObservableForProperty(x => x.ActiveWorkspace)
                    .Select(x => x.GetValue());

            addNoteCmd = 
                CreateCommandForSubtype<IAmAGraph>(currentWorkspaceStream, x => x.NewNoteCommand)
                    .ToProperty(this, x => x.AddNote);

            autoLayoutCmd =
                CreateCommandForSubtype<IAmAGraph>(currentWorkspaceStream, x => x.AutoLayoutCommand)
                    .ToProperty(this, x => x.AutoLayout);

            selectNeighborsCmd =
                CreateCommandForSubtype<IAmAGraph>(currentWorkspaceStream, x => x.SelectNeighborsCommand)
                    .ToProperty(this, x => x.SelectNeighbors);

            deleteSelectedCmd =
                CreateCommandForSubtype<IAmAGraph>(currentWorkspaceStream, x => x.DeleteSelectionCommand)
                    .ToProperty(this, x => x.DeleteSelected);

            selectAllCmd =
                CreateCommandForSubtype<IAmAGraph>(currentWorkspaceStream, x => x.SelectAllCommand)
                    .ToProperty(this, x => x.SelectAll);

            #region Home Workspace Only
            exportSTLCmd =
                CreateCommandForSubtype<ICanExportSTL>(currentWorkspaceStream, x => x.ExportSTLCommand)
                    .ToProperty(this, x => x.ExportSTL);

            runCmd =
                CreateCommandForSubtype<ICanExecute>(currentWorkspaceStream, x => x.RunGraphCommand)
                    .ToProperty(this, x => x.RunGraph);

            forceRunCmd =
                CreateCommandForSubtype<ICanExecute>(currentWorkspaceStream, x => x.ForceRunGraphCommand)
                    .ToProperty(this, x => x.ForceRunGraph);
            #endregion

            #region Custom Node Workspace Only
            publishActiveWorkspaceCmd =
                currentWorkspaceStream.Select(
                    ws =>
                        ws is ICanBePublished
                            ? (ws as ICanBePublished).PublishCommand
                            : ReactiveCommand.Create(Observable.Never<bool>()))
                    .ToProperty(this, x => x.PublishActiveWorkspace);
            #endregion
            #endregion

            var newCustomNodeCmd = ReactiveCommand.Create();
            NewCustomNode = newCustomNodeCmd;
            var newCustomNodeStream = newCustomNodeCmd.Cast<NewCustomNodeEventArgs>();

            var newHomeWorkspaceCmd = ReactiveCommand.Create();
            NewHomeWorkspace = newHomeWorkspaceCmd;
            var newHomeWorkspaceStream = newHomeWorkspaceCmd.Cast<NewHomeWorkspaceEventArgs>();

            var displayStartPageCmd = ReactiveCommand.Create();
            DisplayStartPage = displayStartPageCmd;
            var displayStartPageStream = displayStartPageCmd;

            var consoleStreams = Observable.Merge<LogEntry>();
            Console = new ConsoleViewModel(consoleStreams);
        }

        private static IObservable<ICommand> CreateCommandForSubtype<T>(
            IObservable<object> source,
            Func<T, ICommand> selector)
        {
            return source.Select(ws => ws is T ? selector((T)ws) : inactiveCommand);
        }

        private static readonly ICommand inactiveCommand = ReactiveCommand.Create(Observable.Return(false));

        /// <summary>
        ///     Workspace which all workspace-focused commands act upon.
        /// </summary>
        public object ActiveWorkspace
        {
            get { return activeWorkspace; }
            set { this.RaiseAndSetIfChanged(ref activeWorkspace, value); }
        }
        private object activeWorkspace;


        /// <summary>
        ///     The Dynamo Console.
        /// </summary>
        public ConsoleViewModel Console { get; private set; }

        /// <summary>
        ///     Adds a new, blank note to the active workspace.
        /// </summary>
        public ICommand AddNote { get { return addNoteCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> addNoteCmd; 

        /// <summary>
        ///     Performs an automatic graph layout on the active workspace.
        /// </summary>
        public ICommand AutoLayout { get { return autoLayoutCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> autoLayoutCmd; 

        /// <summary>
        ///     Adds all nodes adjacent to the currently selected nodes to the selection
        ///     in the active workspace.
        /// </summary>
        public ICommand SelectNeighbors { get { return selectNeighborsCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> selectNeighborsCmd;

        /// <summary>
        ///     Selects all selectable objects in the active workspace.
        /// </summary>
        public ICommand SelectAll { get { return selectAllCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> selectAllCmd; 

        /// <summary>
        ///     Deletes all items in the active workspace's selection.
        /// </summary>
        public ICommand DeleteSelected { get { return deleteSelectedCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> deleteSelectedCmd;

        /// <summary>
        ///     Saves an image of the active workspace.
        /// </summary>
        public ICommand SaveWorkspaceImage { get { return saveImageCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> saveImageCmd;

        /// <summary>
        ///     File > Save...
        /// </summary>
        public ICommand SaveWorkspace { get { return saveCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> saveCmd;

        /// <summary>
        ///     File > Save As...
        /// </summary>
        public ICommand SaveWorkspaceAs { get { return saveAsCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> saveAsCmd;

        /// <summary>
        ///     Exports geometry created by the active workspace as an STL file.
        /// </summary>
        //TODO: Because the UI needs to display a dialog, it should probably not be a command.
        public ICommand ExportSTL { get { return exportSTLCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> exportSTLCmd;

        /// <summary>
        ///     Compiles and executes the active workspace.
        /// </summary>
        public ICommand RunGraph { get { return runCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> runCmd;

        /// <summary>
        ///     Compiles and executes the active workspace.
        /// </summary>
        public ICommand ForceRunGraph { get { return forceRunCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> forceRunCmd;

        /// <summary>
        ///     Publishes the active Custom Node workspace to the package manager.
        /// </summary>
        public ICommand PublishActiveWorkspace { get { return publishActiveWorkspaceCmd.Value; } }
        private readonly ObservableAsPropertyHelper<ICommand> publishActiveWorkspaceCmd; 

        /// <summary>
        ///     Creates a new, blank Custom Node workspace with name, description, and category
        ///     taken from the given NewCustomNodeEventArgs argument.
        /// </summary>
        public ICommand NewCustomNode { get; private set; }

        /// <summary>
        ///     Creates a new, blank Home workspace initialized from the given
        ///     NewHomeWorkspaceEventArgs argument.
        /// </summary>
        public ICommand NewHomeWorkspace { get; private set; }

        /// <summary>
        ///     Re-displays the Dynamo Start Page by adding it to the Workspaces collection and
        ///     setting it as the active workspace.
        /// </summary>
        public ICommand DisplayStartPage { get; private set; }

        /* Properties */

        // RunInDebug
        // ConsoleHeight
        // RecentFiles
        // ConnectorType
        // IsShowingConnectors
        // AlternateContextGeometryDisplayText
        // VisualizationManager.AlternateDrawingContextAvailable
        // VisualizationManager.DrawToAlternateContext
        // FullscreenWatchShowing
        // IsUsageReportingApproved
        // IsAnalyticsReportingApproved
        // MaxTesselationDivisions
        // IsDebugBuild
        // VerboseLogging
        // ShowDebugASTs
        // RunEnabled
        // ShouldBeHitTestVisible
        // IsHomeSpace
        // WatchPreviewHitTest
        // CurrentWorkspaceIndex
        // ViewingHomespace
        // LogText
        // DynamicRunEnabled
        // CanRunDynamically



        /* Commands */

        // Active Workspace Forwarding
        //     GraphAutoLayout*
        //     AddNote*
        //     SelectNeighbors*
        //     Delete*
        //     SelectAll*
        //     -----------                         --+
        //     PublishSelectedNodes                  |
        //     ShowSaveImageDialogAndSaveResult      |
        //     ShowSaveDialogIfNeededAndSaveResult   +-- Dialogs should be Views that use the Workspace as a ViewModel (these probably shouldn't even be commands...)
        //     ShowSaveDialogAndSaveResult           |
        //     -----------                         --+
        //     Copy
        //     Paste

        // Home Workspace Forwarding
        //     ExportToSTL*
        //     RunExpression*
        //     ForceRunExpression*
        //     -----------
        //     CancelRun

        // Custom Node Workspace Forwarding
        //     PublishCurrentWorkspace

        // Workspace Creation
        //     NewHomeWorkspace
        //     NewCustomNode*
        //     DisplayStartPage
        
        // Loading
        //     ShowOpenDialogAndOpenResult
        //     OpenRecent
        
        // Command Playback
        //     SaveRecorded
        //     InsertPlayPausePlayback
        //     Undo
        //     Redo
        
        // Console
        //     ToggleConsoleShowing
        //     ClearLog

        // CloseWorkspace (Formerly "Hide")
        // Exit
        // SetConnectorType
        // ShowHideConnectors
        
        // ToggleFullscreenWatchShowing
        // ToggleCanNavigateBackground
        
        // ImportLibrary
        
        // ShowPackageManagerSearch
        // ShowInstalledPackages

        // ToggleIsUsageReportingApproved
        // ToggleIsAnalyticsReportingApproved

        // SetLengthUnit
        // SetAreaUnit
        // SetVolumeUnit
        // SetNumberFormat

        // ReportABug
        // GoToSourceCode
        // GoToWiki
        // ShowAboutWindow
        // MutateTestDelegate
        // UpdateManager.CheckNewerDailyBuilds
        // UpdateManager.ForceUpdate
    }
}