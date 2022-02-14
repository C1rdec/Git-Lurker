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

    public class ShellViewModel : Screen, IHandle<object>
    {
        #region Fields

        private Window _parent;
        private string _searchTerm;
        private DebounceService _debounceService;
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
            _debounceService = new DebounceService();
            _keyboardService = keyboardService;

            var first = settings.Entity.Workspaces.FirstOrDefault();
            if (first != null)
            {
                WorkspaceViewModel = new WorkspaceViewModel(new Workspace(settings.Entity.Workspaces.FirstOrDefault()), keyboardService);
            }

            _eventAggregator.SubscribeOnPublishedThread(this);
            HotkeyManager.Current.AddOrReplace("Increment", Key.G, ModifierKeys.Control , (s, e) => Show());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the workspace view model.
        /// </summary>
        /// <value>The workspace view model.</value>
        public WorkspaceViewModel WorkspaceViewModel { get; private set; }

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

        protected Window View { get; private set; }

        #endregion

        #region Methods

        public void Search(string term)
        {
            if (WorkspaceViewModel == null)
            {
                return;
            }

            WorkspaceViewModel.Search(term);
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
        }

        public async void Show()
        {
            IsVisible = true;
            DockingHelper.SetForeground(View);
        }

        protected override async void OnViewLoaded(object view)
        {
            View = view as Window;
            await Task.Delay(200);
            await _keyboardService.InstallAsync();

            // Needs to be done after Winook
            ShowInTaskBar = false;
            HideFromAltTab(View);
        }

        private void HideFromAltTab(Window view)
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
            view.Owner = this._parent;
            this._parent.Hide();
        }

        #endregion
    }
}
