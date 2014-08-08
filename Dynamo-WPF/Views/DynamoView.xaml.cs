using System.Windows.Controls;

using Xceed.Wpf.AvalonDock.Layout;

namespace Dynamo.UI.Wpf.Views
{
    /// <summary>
    /// Interaction logic for DynamoView.xaml
    /// </summary>
    public partial class DynamoView : UserControl
    {
        public ILayoutUpdateStrategy LayoutStrategy { get; private set; } 

        public DynamoView()
        {
            InitializeComponent();
            LayoutStrategy = new LayoutUpdateStrategy();
            
            // Insert code required on object creation below this point.
        }

        //TODO: Smarter Layout logic
        private class LayoutUpdateStrategy : ILayoutUpdateStrategy
        {
            public bool BeforeInsertAnchorable(
                LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
            {
                return true;
            }

            public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
            {
            }

            public bool BeforeInsertDocument(
                LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
            {
                return true;
            }

            public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
            {
            }
        }
    }
}