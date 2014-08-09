using System.Reactive.Linq;
using System.Windows.Input;

using Dynamo.UI.Models;

using ReactiveUI;

namespace Dynamo.UI.Wpf.ViewModels
{
    public class DynamoViewModel : ViewModelBase
    {
        public interface ICanExportSTL
        {
            ICommand ExportSTLCommand { get; }
        }

        public interface ICanExecute 
        {
            ICommand RunGraphCommand { get; }
            ICommand ForceRunGraphCommand { get; }
        }

        public interface ICanBePublished
        {
            ICommand PublishCommand { get; }
        }

        public DynamoViewModel()
        {
            #region Initialize Active Workspace Operations
            var currentWorkspaceStream =
                this.ObservableForProperty(x => x.ActiveWorkspace, skipInitial: false)
                    .Select(x => x.GetValue());

            addNoteCmd = 
                currentWorkspaceStream.Select(x => x.NewNoteCommand)
                    .ToProperty(this, x => x.AddNote);

            autoLayoutCmd = 
                currentWorkspaceStream.Select(x => x.AutoLayoutCommand)
                    .ToProperty(this, x => x.AutoLayout);

            selectNeighborsCmd = 
                currentWorkspaceStream.Select(x => x.SelectNeighborsCommand)
                    .ToProperty(this, x => x.SelectNeighbors);

            deleteSelectedCmd =
                currentWorkspaceStream.Select(x => x.DeleteSelectionCommand)
                    .ToProperty(this, x => x.DeleteSelected);

            selectAllCmd = 
                currentWorkspaceStream.Select(x => x.SelectAllCommand)
                    .ToProperty(this, x => x.SelectAll);

            #region Home Workspace Only
            exportSTLCmd =
                currentWorkspaceStream.Select(
                    ws =>
                        ws is ICanExportSTL
                            ? (ws as ICanExportSTL).ExportSTLCommand
                            : ReactiveCommand.Create(Observable.Never<bool>()))
                    .ToProperty(this, x => x.ExportSTL);

            runCmd =
                currentWorkspaceStream.Select(
                    ws =>
                        ws is ICanExecute
                            ? (ws as ICanExecute).RunGraphCommand
                            : ReactiveCommand.Create(Observable.Never<bool>()))
                    .ToProperty(this, x => x.RunGraph);

            forceRunCmd =
                currentWorkspaceStream.Select(
                    ws =>
                        ws is ICanExecute
                            ? (ws as ICanExecute).ForceRunGraphCommand
                            : ReactiveCommand.Create(Observable.Never<bool>()))
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

            var consoleStreams = Observable.Merge<LogEntry>();
            Console = new ConsoleViewModel(consoleStreams);
        }

        /// <summary>
        ///     Workspace which all workspace-focused commands act upon.
        /// </summary>
        public WorkspaceViewModel ActiveWorkspace
        {
            get { return activeWorkspace; }
            set { this.RaiseAndSetIfChanged(ref activeWorkspace, value); }
        }
        private WorkspaceViewModel activeWorkspace;

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
        //     RunExpression
        //     ForceRunExpression
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