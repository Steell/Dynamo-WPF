using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Dynamo.UI.Models;
using ObservableExtensions;
using ReactiveUI;
using DynamoModel = Dynamo.UI.Models.Dynamo;

namespace Dynamo.UI.Wpf.ViewModels
{
    public class DynamoViewModel : ViewModelBase
    {
        public const int RECENT_FILE_AMOUNT = 5;

        public DynamoViewModel()
        {
            #region Active Workspace Operations
            var activeWorkspaceStream =
                this.ObservableForProperty(x => x.ActiveWorkspace)
                    .Select(x => x.GetValue());

            #region Graph Workspaces Only
            addNoteCmd = 
                CreateCommandForSubtype<IGraphViewModel>(activeWorkspaceStream, x => x.NewNoteCommand)
                    .ToProperty(this, x => x.AddNote);

            autoLayoutCmd =
                CreateCommandForSubtype<IGraphViewModel>(activeWorkspaceStream, x => x.AutoLayoutCommand)
                    .ToProperty(this, x => x.AutoLayout);

            selectNeighborsCmd =
                CreateCommandForSubtype<IGraphViewModel>(activeWorkspaceStream, x => x.SelectNeighborsCommand)
                    .ToProperty(this, x => x.SelectNeighbors);

            deleteSelectedCmd =
                CreateCommandForSubtype<IGraphViewModel>(activeWorkspaceStream, x => x.DeleteSelectionCommand)
                    .ToProperty(this, x => x.DeleteSelected);

            selectAllCmd =
                CreateCommandForSubtype<IGraphViewModel>(activeWorkspaceStream, x => x.SelectAllCommand)
                    .ToProperty(this, x => x.SelectAll);

            var addNodeCmdStream = CreateCommandForSubtype<IGraphViewModel>(activeWorkspaceStream, x => x.NewNodeCommand);
            #endregion
            #region Home Workspace Only
            exportSTLCmd =
                CreateCommandForSubtype<ICanExportSTL>(activeWorkspaceStream, x => x.ExportSTLCommand)
                    .ToProperty(this, x => x.ExportSTL);

            runCmd =
                CreateCommandForSubtype<ICanExecute>(activeWorkspaceStream, x => x.RunGraphCommand)
                    .ToProperty(this, x => x.RunGraph);

            forceRunCmd =
                CreateCommandForSubtype<ICanExecute>(activeWorkspaceStream, x => x.ForceRunGraphCommand)
                    .ToProperty(this, x => x.ForceRunGraph);
            #endregion
            #region Custom Node Workspace Only
            publishActiveWorkspaceCmd =
                CreateCommandForSubtype<ICanBePublished>(activeWorkspaceStream, x => x.PublishCommand)
                    .ToProperty(this, x => x.PublishActiveWorkspace);
            #endregion
            #endregion

            var newCustomNodeCmd = ReactiveCommand.Create();
            NewCustomNode = newCustomNodeCmd;
            var newCustomNodeStream = newCustomNodeCmd.Cast<NewCustomNodeEventArgs>();

            var newHomeWorkspaceCmd = ReactiveCommand.Create();
            NewHomeWorkspace = newHomeWorkspaceCmd;
            var newHomeWorkspaceStream = newHomeWorkspaceCmd.Cast<NewHomeWorkspaceEventArgs>();

            var displayStartPageCmd =
                ReactiveCommand.Create(
                    activeWorkspaceStream.Select(x => !(x is StartPageViewModel)));
            DisplayStartPage = displayStartPageCmd;
            var displayStartPageStream = displayStartPageCmd.Select(_ => Unit.Default);

            var openWorkspaceCmd = ReactiveCommand.Create();
            OpenWorkspace = openWorkspaceCmd;
            var openWorkspaceStream = openWorkspaceCmd.Cast<string>();

            var showConsoleCmd = ReactiveCommand.Create();
            ShowConsole = showConsoleCmd;
            var showConsoleStream = showConsoleCmd;

            var exitDynamoCmd = ReactiveCommand.Create();
            ExitDynamo = exitDynamoCmd;
            var exitDynamoStream = exitDynamoCmd;

            var importLibraryCmd = ReactiveCommand.Create();
            ImportLibrary = importLibraryCmd;
            var importLibraryStream = importLibraryCmd.Cast<string>();

            var runAutoStream =
                this.ObservableForProperty(x => x.RunningAutomatically).Select(x => x.GetValue());

            var consoleStreams = Observable.Merge<LogEntry>().StartWith(new LogEntry("Welcome To Dynamo!"));

            var model = new DynamoModel(
                customNodes: newCustomNodeStream,
                homeWorkspaces: newHomeWorkspaceStream,
                displayStartPage: displayStartPageStream,
                openWorkspace: openWorkspaceStream,
                importLibrary: importLibraryStream,
                runAuto: runAutoStream
            );

            recentFiles =
                model.RecentFiles.BufferOverlap(RECENT_FILE_AMOUNT)
                    .Select(x => x.ToList())
                    .ToProperty(this, x => x.RecentFiles);

            //TODO: these should be initialized from the model
            Console = new ConsoleViewModel(consoleStreams);
            Library = new NodeLibraryViewModel();
        }

        #region Properties
        // RunInDebug TODO: move to workspace
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
        #endregion
        #region Commands
        // Active Workspace Forwarding
        //     -----------                         --+
        //     PublishSelectedNodes                  |
        //     ShowSaveImageDialogAndSaveResult      |
        //     ShowSaveDialogIfNeededAndSaveResult   +-- Dialogs should be Views that use the Workspace as a ViewModel (these probably shouldn't even be commands...)
        //     ShowSaveDialogAndSaveResult           |
        //     -----------                         --+
        //     Copy
        //     Paste

        // Home Workspace Forwarding
        //     CancelRun
        
        // Loading
        //     OpenRecent TODO: expost a list of recent files, UI feeds elements from list back into Open command.

        // Command Playback
        //     SaveRecorded
        //     InsertPlayPausePlayback
        //     Undo
        //     Redo
        
        // CloseWorkspace (Formerly "Hide") TODO: this should just watch for changes in the observable collection of workspaces
        // SetConnectorType
        // ShowHideConnectors

        // ToggleFullscreenWatchShowing
        // ToggleCanNavigateBackground

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
        #endregion

        #region Private Helpers
        private static IObservable<ICommand> CreateCommandForSubtype<T>(
            IObservable<object> source,
            Func<T, ICommand> selector)
        {
            return source.Select(ws => ws is T ? selector((T)ws) : inactiveCommand);
        }

        private static readonly ICommand inactiveCommand = ReactiveCommand.Create(Observable.Return(false));
        #endregion

        /// <summary>
        ///     List of recently opened files.
        /// </summary>
        public List<string> RecentFiles { get { return recentFiles.Value; } }
        private readonly ObservableAsPropertyHelper<List<string>> recentFiles;

        /// <summary>
        ///     Determines if the active workspace is running automatically.
        ///     TODO: This belongs on home workspace.
        /// </summary>
        public bool RunningAutomatically
        {
            get { return runAuto; }
            set { this.RaiseAndSetIfChanged(ref runAuto, value); }
        }
        private bool runAuto;

        /// <summary>
        ///     Workspace which all workspace-focused commands act upon.
        /// </summary>
        // TODO: Model shouldn't report active workspace? It will receive a stream specifying the active workspace.
        public AWorkspaceViewModel ActiveWorkspace
        {
            get { return activeWorkspace; }
            set { this.RaiseAndSetIfChanged(ref activeWorkspace, value); }
        }
        private AWorkspaceViewModel activeWorkspace;
        
        /// <summary>
        ///     The Dynamo Console.
        /// </summary>
        public ConsoleViewModel Console { get; private set; }

        /// <summary>
        ///     The Node Library.
        /// </summary>
        public NodeLibraryViewModel Library { get; private set; }

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
        ///     Cancels an in-progress execution of the active workspace.
        /// </summary>
        public ICommand CancelRun { get; private set; }

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
        ///     Re-displays the Dynamo Start Page by adding it to the Workspaces collection (if 
        ///     necessary) and setting it as the active workspace.
        /// </summary>
        public ICommand DisplayStartPage { get; private set; }

        /// <summary>
        ///     Opens a workspace from a file. The path to the file is given as a string argument.
        /// </summary>
        public ICommand OpenWorkspace { get; private set; }

        /// <summary>
        ///     Displays the Dynamo Console.
        /// </summary>
        public ICommand ShowConsole { get; private set; }

        /// <summary>
        ///     Exits Dynamo.
        /// </summary>
        public ICommand ExitDynamo { get; private set; }

        /// <summary>
        ///     Imports nodes into the Dynamo Library from a DLL. The path the the DLL file is given
        ///     as a string argument.
        /// </summary>
        public ICommand ImportLibrary { get; private set; }
    }
}