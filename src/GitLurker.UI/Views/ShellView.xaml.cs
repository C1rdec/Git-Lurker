using System.Windows;

namespace GitLurker.UI.Views
{
    public partial class ShellView : Window
    {
        #region Fields

        private Window _parent;

        #endregion

        public ShellView()
        {
            this.InitializeComponent();
            this.HideFromAltTab();

        }

        /// <summary>
        /// Hides the window from alt tab.
        /// </summary>
        private void HideFromAltTab()
        {
            this._parent = new Window
            {
                Top = -100,
                Left = -100,
                Width = 1,
                Height = 1,

                WindowStyle = WindowStyle.ToolWindow, // Set window style as ToolWindow to avoid its icon in AltTab
                ShowInTaskbar = false,
            };

            this._parent.Show();
            this.Owner = this._parent;
            this._parent.Hide();
        }
    }
}
