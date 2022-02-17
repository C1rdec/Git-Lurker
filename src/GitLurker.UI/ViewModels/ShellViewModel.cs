namespace GitLurker.UI.ViewModels
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;
    using GitLurker.UI.Models;
    using NHotkey.Wpf;
    using System.Windows.Input;
    using System.Windows;
    using GitLurker.UI.Helpers;
    using GitLurker.UI.Views;

    public class ShellViewModel : Screen, IHandle<object>
    {
        #region Fields

        private Window _parent;
        private string _searchTerm;
        private KeyboardService _keyboardService;
        private bool _isVisible;
        private bool _showInTaskBar;
        private IEventAggregator _eventAggregator;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel(IEventAggregator aggregator, SettingsFile settings, KeyboardService keyboardService)
        {
            _searchTerm = string.Empty;
            _isVisible = false;
            _showInTaskBar = true;
            _eventAggregator = aggregator;
            _keyboardService = keyboardService;

            var worskspacePaths = settings.Entity.Workspaces;
            if (worskspacePaths.Any())
            {
                var worskspaces = worskspacePaths.Select(w => new Workspace(w)).ToArray();
                WorkspaceViewModel = new WorkspaceViewModel(worskspaces, keyboardService);
            }

            _eventAggregator.SubscribeOnPublishedThread(this);
            HotkeyManager.Current.AddOrReplace("Open", Key.G, ModifierKeys.Control , (s, e) => ShowWindow());
        }

        #endregion

        #region Properties

        public WorkspaceViewModel WorkspaceViewModel { get; init; }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm == value)
                {
                    return;

                }
                _searchTerm = value;
                Search(_searchTerm);
                NotifyOfPropertyChange();
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public bool ShowInTaskBar
        {
            get => _showInTaskBar;
            set
            {
                _showInTaskBar = value;
                NotifyOfPropertyChange();
            }
        }

        protected ShellView View { get; private set; }

        #endregion

        #region Methods

        public void Search(string term)
        {
            WorkspaceViewModel?.Search(term);
        }

        public Task HandleAsync(object message, CancellationToken cancellationToken)
        {
            IsVisible = false;
            return Task.CompletedTask;
        }

        public void HideWindow()
        {
            IsVisible = false;
            SearchTerm = string.Empty;
            WorkspaceViewModel?.Clear();
        }

        public void ShowWindow()
        {
            IsVisible = true;
            DockingHelper.SetForeground(View, () => View.SearchTerm.Focus());
        }

        protected override async void OnViewLoaded(object view)
        {
            View = view as ShellView;
            await Task.Delay(200);
            await _keyboardService.InstallAsync();

            // Needs to be done after Winook
            ShowInTaskBar = false;
            HideFromAltTab(View);
        }

        private void HideFromAltTab(Window view)
        {
            _parent = new Window
            {
                Top = -100,
                Left = -100,
                Width = 1,
                Height = 1,

                // Set window style as ToolWindow to avoid its icon in AltTab
                WindowStyle = WindowStyle.ToolWindow, 
                ShowInTaskbar = false,
            };

            _parent.Show();
            view.Owner = this._parent;
            _parent.Hide();
        }

        #endregion
    }
}
