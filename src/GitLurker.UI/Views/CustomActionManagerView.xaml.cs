namespace GitLurker.UI.Views;

using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for CustomActionManagerView.xaml
/// </summary>
public partial class CustomActionManagerView : UserControl
{
    public CustomActionManagerView()
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
