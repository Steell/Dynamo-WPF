using System;
using System.Windows.Input;

namespace Dynamo.UI.Wpf.ViewModels
{
    public interface IGraphViewModel
    {
        ICommand NewNoteCommand { get; }
        ICommand AutoLayoutCommand { get; }
        ICommand SelectNeighborsCommand { get; }
        ICommand DeleteSelectionCommand { get; }
        ICommand SelectAllCommand { get; }
        ICommand NewNodeCommand { get; }
    }

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
    
    public interface ISelectable
    {
        IObservable<bool> SelectionChangedStream { get; }
    }
}
