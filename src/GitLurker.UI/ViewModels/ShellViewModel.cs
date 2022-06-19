namespace GitLurker.UI.ViewModels
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;
    using GitLurker.UI.Helpers;
    using GitLurker.UI.Messages;
    using GitLurker.UI.Services;
    using GitLurker.UI.Views;
    using NHotkey.Wpf;
    using WindowsUtilities;

    public class ShellViewModel : Screen, IHandle<CloseMessage>, IHandle<string>, IHandle<ConsoleMessage>, IDisposable
    {
        #region Fields

        private static readonly string DefaultWaterMark = "Search";
        private Window _parent;
        private SettingsFile _settingsFile;
        private KeyboardService _keyboardService;
        private RepositoryService _repositoryService;
        private SurfaceDialService _surfaceDialService;
        private GithubUpdateManager _updateManager;
        private WindowsLink _startupService;
        private ConsoleViewModel _console;
        private string _searchTerm;
        private string _searchWatermark;
        private bool _isVisible;
        private bool _showInTaskBar;
        private bool _disable;
        private IEventAggregator _eventAggregator;
        private bool _topMost;
        private string _version;
        private double _dpiX = 1;
        private double _dpiY = 1;
        private bool _hasSurfaceDial;
        private bool _isConsoleOpen;
        private bool _needUpdate;
        private string _consoleHeader;

        #endregion

        #region Constructors

        public ShellViewModel(
            IEventAggregator aggregator,
            SettingsFile settings,
            KeyboardService keyboardService,
            WindowsLink startupService,
            RepositoryService repositoryService,
            SurfaceDialService surfaceDialService,
            ThemeService themeService,
            GithubUpdateManager updateManager,
            ConsoleViewModel console)
        {
            _console = console;
            _searchTerm = string.Empty;
            _searchWatermark = DefaultWaterMark;
            _isVisible = false;
            _showInTaskBar = true;
            _eventAggregator = aggregator;
            _keyboardService = keyboardService;
            _startupService = startupService;
            _repositoryService = repositoryService;
            _surfaceDialService = surfaceDialService;
            _settingsFile = settings;
            _updateManager = updateManager;

            _updateManager.UpdateRequested += UpdateManager_UpdateRequested;
            _settingsFile.OnFileSaved += OnSettingsSave;

            ApplySettings(settings);

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            _version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

            RefreshWorkspace();

            SetGlobalHotkey();
            _eventAggregator.SubscribeOnPublishedThread(this);
            themeService.Apply();
        }

        #endregion

        #region Properties

        public DoubleClickCommand ShowSettings => new DoubleClickCommand(OpenSettings);

        public WorkspaceViewModel WorkspaceViewModel { get; private set; }

        public ConsoleViewModel Console => _console;

        public bool ShowConsoleOutput => _settingsFile.Entity.ConsoleOuput && UpToDate;

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

        public bool NeedUpdate
        {
            get => _needUpdate;
            set
            {
                _needUpdate = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => UpToDate);
                NotifyOfPropertyChange(() => ShowConsoleOutput);
            }
        }

        public bool UpToDate => !NeedUpdate;

        public bool HasSurfaceDial
        {
            get => _hasSurfaceDial;
            set
            {
                _hasSurfaceDial = value;
                NotifyOfPropertyChange();
            }
        }

        public string SearchWatermark
        {
            get => _searchWatermark;
            set
            {
                _searchWatermark = value;
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

        public bool TopMost
        {
            get => _topMost;
            set
            {
                _topMost = value;
                NotifyOfPropertyChange();
            }
        }

        public bool Disable
        {
            get => _disable;
            set
            {
                _disable = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsConsoleOpen
        {
            get => _isConsoleOpen;
            set
            {
                _isConsoleOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public string ConsoleHeader
        {
            get => string.IsNullOrEmpty(_consoleHeader) ? "Console output" : _consoleHeader;
            set
            {
                _consoleHeader = value;
                NotifyOfPropertyChange();
            }
        }

        public string Version => _version;

        protected ShellView View { get; private set; }

        #endregion

        #region Methods

        public void Search(string term)
        {
            WorkspaceViewModel?.Search(term);
        }

        public void OpenConsole() => IsConsoleOpen = true;

        public Task HandleAsync(CloseMessage message, CancellationToken cancellationToken)
        {
            IsVisible = false;
            return Task.CompletedTask;
        }

        public async Task HandleAsync(string message, CancellationToken cancellationToken)
        {
            SearchWatermark = message;
            await Task.Delay(1600);
            SearchWatermark = DefaultWaterMark;
            FocusSearch();
        }

        public Task HandleAsync(ConsoleMessage message, CancellationToken cancellationToken)
        {
            if (message.Invalid)
            {
                return Task.CompletedTask;
            }

            ConsoleHeader = message.ActionName;
            _console.Initialize(message.Repository);

            if (message.OpenConsole)
            {
                IsConsoleOpen = true;
            }

            return Task.CompletedTask;
        }

        public void HideWindow()
        {
            TopMost = false;
            IsVisible = false;
            SearchTerm = string.Empty;
            WorkspaceViewModel?.Clear();
        }

        public async void Close()
        {
            _parent.Close();
            await TryCloseAsync();
        }

        public void OpenSettings()
        {
            var viewModel = IoC.Get<SettingsViewModel>();
            if (viewModel.IsActive)
            {
                return;
            }

            IoC.Get<IWindowManager>().ShowWindowAsync(IoC.Get<SettingsViewModel>());
        }

        public async void RefreshWorkspace()
        {
            if (WorkspaceViewModel == null)
            {
                WorkspaceViewModel = new WorkspaceViewModel(_keyboardService, _repositoryService);
            }
            else
            {
                _settingsFile.Initialize();
            }

            var worskspacePaths = _settingsFile.Entity.Workspaces;
            if (worskspacePaths.Any())
            {
                Disable = true;
                SearchTerm = string.Empty;
                SearchWatermark = "Loading...";

                WorkspaceViewModel.Clear();
                await WorkspaceViewModel.RefreshRepositories();
                var gitLurkerRepo = _repositoryService.GetAllRepo().FirstOrDefault(r => r.Name == "GitLurker");
                if (gitLurkerRepo != null)
                {
                    _updateManager.WatchAsync(gitLurkerRepo);
                }

                Disable = false;
                SearchWatermark = DefaultWaterMark;

                FocusSearch();
                WorkspaceViewModel.ShowRecent();
            }
        }

        public async void Update()
        {
            Dispose();
            await _updateManager.Update();
        }

        protected override async void OnViewLoaded(object view)
        {
            View = view as ShellView;

            await _surfaceDialService.Initialize(View);

            _surfaceDialService.ButtonClicked += SurfaceDialService_ButtonClicked;
            _surfaceDialService.RotatedRight += SurfaceDialService_RotatedRight;
            _surfaceDialService.RotatedLeft += SurfaceDialService_RotatedLeft;
            _surfaceDialService.ButtonHolding += SurfaceDialService_ButtonHolding;
            _surfaceDialService.ControlAcquired += SurfaceDialService_ControlAcquired;
            _surfaceDialService.ControlLost += SurfaceDialService_ControlLost;

            var source = PresentationSource.FromVisual(this.View);
            if (source != null)
            {
                _dpiX = source.CompositionTarget.TransformToDevice.M11;
                _dpiY = source.CompositionTarget.TransformToDevice.M22;
            }

            await Task.Delay(200);
            await _keyboardService.InstallAsync();

            // Needs to be done after Winook
            ShowInTaskBar = false;
            HideFromAltTab(View);
        }

        private void UpdateManager_UpdateRequested(object sender, EventArgs e) => NeedUpdate = true;

        private void SurfaceDialService_ControlLost(object sender, EventArgs e) => HasSurfaceDial = false;

        private void SurfaceDialService_ControlAcquired(object sender, EventArgs e) => HasSurfaceDial = true;

        private void SurfaceDialService_ButtonHolding(object sender, EventArgs e) => WorkspaceViewModel.OpenPullRequest();

        private void SurfaceDialService_RotatedLeft(object sender, EventArgs e) => WorkspaceViewModel.MoveUp();

        private void SurfaceDialService_RotatedRight(object sender, EventArgs e) => WorkspaceViewModel.MoveDown();

        private void SurfaceDialService_ButtonClicked(object sender, EventArgs e) => WorkspaceViewModel.Open(); 

        private void ToggleWindow()
        {
            if (IsVisible)
            {
                if (_settingsFile.Entity.DoubleTabEnabled)
                {
                    WorkspaceViewModel?.Open(skipModifier: true);
                }

                HideWindow();
                return;
            }

            HandleScreenPosition();
            WorkspaceViewModel?.ShowRecent();

            TopMost = true;
            IsVisible = true;
            FocusSearch();
        }

        private void FocusSearch() => DockingHelper.SetForeground(View, () =>
        {
            View.SearchTerm.Focus();
        }); 

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
            view.Owner = _parent;
            _parent.Hide();
        }

        private void OnSettingsSave(object sender, EventArgs e)
        {
            SetGlobalHotkey();
            NotifyOfPropertyChange(() => ShowConsoleOutput);
        }

        private void SetGlobalHotkey()
        {
            var settings = new SettingsFile();
            settings.Initialize();
            var hotkey = settings.Entity.HotKey;
            var modifier = Enum.Parse<ModifierKeys>(hotkey.Modifier.ToString());

            Enum.TryParse(hotkey.KeyCode.ToString(), out Key key);

            try
            {
                HotkeyManager.Current.AddOrReplace("Open", key, modifier , (s, e) => ToggleWindow());
                HotkeyManager.Current.AddOrReplace("OpenDial", Key.F12, ModifierKeys.Control | ModifierKeys.Shift, (s, e) => ToggleWindow());
            }
            catch (NHotkey.HotkeyAlreadyRegisteredException)
            {
            }
        }

        private void ApplySettings(SettingsFile settings)
        {
            if (settings.Entity.StartWithWindows)
            {
                _startupService.AddStartMenu();
            }
            else
            {
                _startupService.RemoveStartMenu();
            }

            if (settings.Entity.AddToStartMenu)
            {
                _startupService.AddStartWithWindows();
            }
            else
            {
                _startupService.RemoveStartWithWindows();
            }
        }

        private void HandleScreenPosition()
        {
            Execute.OnUIThread(() =>
            {
                var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
                var top = (primaryScreen.Bounds.Height / 2) - (View.Height / 2);
                var left = (primaryScreen.Bounds.Width / 2) - (View.Width / 2);
                View.Top = top / _dpiY;
                View.Left = left / _dpiX;
            });
        }

        public void Dispose()
        {
            _keyboardService?.Dispose();
            _surfaceDialService?.Dispose();

            _surfaceDialService.ButtonClicked -= SurfaceDialService_ButtonClicked;
            _surfaceDialService.RotatedRight -= SurfaceDialService_RotatedRight;
            _surfaceDialService.RotatedLeft -= SurfaceDialService_RotatedLeft;
            _surfaceDialService.ButtonHolding -= SurfaceDialService_ButtonHolding;
            _surfaceDialService.ControlAcquired -= SurfaceDialService_ControlAcquired;
            _surfaceDialService.ControlLost -= SurfaceDialService_ControlLost;
        }

        #endregion
    }
}
