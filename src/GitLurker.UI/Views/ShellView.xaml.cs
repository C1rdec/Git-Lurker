using System;
using System.Diagnostics;
using System.Windows;

namespace GitLurker.UI.Views
{
    public partial class ShellView : Window
    {
        public ShellView()
        {
            this.InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.SearchTerm.Focus();
            Debug.WriteLine("Focus");
        }
    }
}
