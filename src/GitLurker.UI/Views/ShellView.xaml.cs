using System;
using System.Windows;

namespace GitLurker.UI.Views
{
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            SearchTerm.Focus();
        }
    }
}
