namespace GitLurker.UI.Views;

using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for SnippetManagerView.xaml
/// </summary>
public partial class SnippetManagerView : UserControl
{
    public SnippetManagerView()
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
