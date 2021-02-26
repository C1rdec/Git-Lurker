namespace GitLurker.UI.Views
{
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for WorkspaceView.xaml
    /// </summary>
    public partial class WorkspaceView : UserControl
    {
        public WorkspaceView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
