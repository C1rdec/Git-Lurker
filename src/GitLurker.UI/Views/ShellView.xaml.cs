using System.Windows.Input;
using MahApps.Metro.Controls;

namespace GitLurker.UI.Views
{
    public partial class ShellView : MetroWindow
    {
        public ShellView()
        {
            InitializeComponent();
            PreviewKeyDown  += ShellView_PreviewKeyDown;
        }

        private void ShellView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }
        }
    }
}
