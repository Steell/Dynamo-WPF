using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

namespace Dynamo.UI.Wpf.ViewModels
{
	public class DynamoViewModel : INotifyPropertyChanged
	{
		public DynamoViewModel()
		{
			Workspaces = new ObservableCollection<WorkspaceViewModel>();
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion
		
	    private ObservableCollection<WorkspaceViewModel> workspaces;
	    public ObservableCollection<WorkspaceViewModel> Workspaces
	    {
	        get { return workspaces; }
	        set
	        {
	            workspaces = value;
	            NotifyPropertyChanged("Workspaces");
	        }
	    }

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