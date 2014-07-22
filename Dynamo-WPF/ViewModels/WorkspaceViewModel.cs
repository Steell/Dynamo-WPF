using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

namespace Dynamo.UI.Wpf.ViewModels
{
	public class WorkspaceViewModel : INotifyPropertyChanged
	{
		public WorkspaceViewModel()
		{
			
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
		
		private string name;
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
				NotifyPropertyChanged("Name");
			}
		}

        /* Properties */

        // IsCurrentSpace -- Is this the active workspace?
        // CurrentCursor -- Cursor used for ZoomBorder
        // IsCursorForced -- ForceCursor used for ZoomBorder
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
	}
}