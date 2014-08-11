using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dynamo.UI.Wpf.ViewModels
{
    public interface IAmAGraph
    {
        ICommand NewNoteCommand { get; }
        ICommand AutoLayoutCommand { get; }
        ICommand SelectNeighborsCommand { get; }
        ICommand DeleteSelectionCommand { get; }
        ICommand SelectAllCommand { get; }
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
}
