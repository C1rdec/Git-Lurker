namespace GitLurker.UI.ViewModels
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;
    using NHotkey.Wpf;
    using System.Windows.Input;
    using System.Windows;
    using GitLurker.UI.Helpers;
    using GitLurker.UI.Views;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using WindowsUtilities;
    using GitLurker.UI.Helper;

    public class ShellViewModel : Screen, IHandle<object>
    {
        #region Fields

        private Window _parent;
        private SettingsFile _settingsFile;
        private KeyboardService _keyboardService;
        private WindowsLink _startupService;
        private string _searchTerm;
        private string _searchWatermark;
        private bool _isVisible;
        private bool _showInTaskBar;
        private bool _disable;
        private IEventAggregator _eventAggregator;
        private bool _topMost;
        private string _version;
        private bool _isCopiedOpen;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel(IEventAggregator aggregator, SettingsFile settings, KeyboardService keyboardService, WindowsLink startupService)
        {
            _searchTerm = string.Empty;
            _searchWatermark = "Search";
            _isVisible = false;
            _showInTaskBar = true;
            _eventAggregator = aggregator;
            _keyboardService = keyboardService;
            _settingsFile = settings;
            _startupService = startupService;

            ApplySettings(settings);

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            _version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

            RefreshWorkspace();

            SetGlobalHotkey();
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        #endregion

        #region Properties

        public DoubleClickCommand ShowSettings => new DoubleClickCommand(OpenSettings);

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

        public string Version => _version;

        public bool IsCopiedOpen
        {
            get
            {
                return _isCopiedOpen;
            }

            set
            {
                _isCopiedOpen = value;
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
            IoC.Get<IWindowManager>().ShowWindowAsync(new SettingsViewModel(OnSettingsSave));
        }

        public async void RefreshWorkspace()
        {
            if (WorkspaceViewModel == null)
            {
                WorkspaceViewModel = new WorkspaceViewModel(_keyboardService);
            }
            else
            {
                _settingsFile.Initialize();
            }

            var worskspacePaths = _settingsFile.Entity.Workspaces;
            if (worskspacePaths.Any())
            {
                Disable = true;
                SearchTerm = String.Empty;
                SearchWatermark = "Loading...";

                WorkspaceViewModel.Clear();
                var worskspaces = await GetWorkspaces(worskspacePaths);
                WorkspaceViewModel.Refresh(worskspaces);

                Disable = false;
                SearchWatermark = "Search";
                HideWindow();
            }
        }

        public void Share()
        {
            IsCopiedOpen = true;
            Task.Delay(1400).ContinueWith(t => IsCopiedOpen = false);
            ClipboardHelper.SetText("https://github.com/C1rdec/Git-Lurker");
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

        private Task<Workspace[]> GetWorkspaces(IEnumerable<string> paths) => Task.Run(() => paths.Select(w => new Workspace(w)).ToArray());

        private void ToggleWindow()
        {
            if (IsVisible)
            {
                HideWindow();
                return;
            }

            TopMost = true;
            IsVisible = true;
            DockingHelper.SetForeground(View, () => View.SearchTerm.Focus());

            WorkspaceViewModel?.ShowRecent();
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

        private void OnSettingsSave()
        {
            SetGlobalHotkey();
            RefreshWorkspace();
        }

        private void SetGlobalHotkey()
        {
            var settings = new SettingsFile();
            settings.Initialize();
            var hotkey = settings.Entity.HotKey;
            var modifier = Enum.Parse<ModifierKeys>(hotkey.Modifier.ToString());

            Enum.TryParse(hotkey.KeyCode.ToString(), out Key key);

            HotkeyManager.Current.AddOrReplace("Open", key, modifier , (s, e) => ToggleWindow());
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

        #endregion
    }
}
