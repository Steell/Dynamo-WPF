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
            var currentWorkspaceStream =
                this.ObservableForProperty(
                    x => x.ActiveWorkspace,
                    skipInitial: false)
                .Select(x => x.GetValue());

            addNoteCmd = 
                currentWorkspaceStream.Select(x => x.NewNoteCommand)
                    .ToProperty(this, x => x.AddNote);

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

        private readonly ObservableAsPropertyHelper<ICommand> addNoteCmd; 
        public ICommand AddNote { get { return addNoteCmd.Value; } }

        public ConsoleViewModel Console { get; private set; }

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

        // SelectNeighbors
        // Delete
        // NewHomeWorkspace
        // ShowNewFunctionDialog
        // SaveRecorded
        // InsertPlayPausePlayback
        // GraphAutoLayout
        // SelectNeighbors
        // Copy
        // Paste
        // Undo
        // Redo
        // SelectAll
        // ToggleCanNavigateBackground
        // AddNote
        // ShowSaveDialogIfNeededAndSaveResult
        // ShowSaveDialogAndSaveResult
        // ShowOpenDialogAndOpenResult
        // Home
        // ToggleConsoleShowing
        // Exit
        // RunExpression
        // ShowSaveImageDialogAndSaveResult
        // ExportToSTL
        // OpenRecent
        // SetConnectorType
        // ShowHideConnectors
        // ToggleFullscreenWatchShowing
        // ToggleCanNavigateBackground
        // ImportLibrary
        // ShowPackageManagerSearch
        // ShowInstalledPackages
        // PublishSelectedNodes
        // PublishCurrentWorkspace
        // ToggleIsUsageReportingApproved
        // ToggleIsAnalyticsReportingApproved
        // SetLengthUnit
        // SetAreaUnit
        // SetVolumeUnit
        // SetNumberFormat
        // ReportABug
        // GoToSourceCode
        // GoToWiki
        // DisplayStartPage
        // ShowAboutWindow
        // ForceRunExpression
        // MutateTestDelegate
        // UpdateManager.CheckNewerDailyBuilds
        // UpdateManager.ForceUpdate
        // Hide
        // ClearLog
        // RunExpression
        // CancelRun
    }
}