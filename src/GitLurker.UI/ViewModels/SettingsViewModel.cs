using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.Services;
using GitLurker.UI.Models;
using GitLurker.UI.Services;
using LibGit2Sharp;
using Lurker.Epic.Services;
using Lurker.Steam.Services;
using Lurker.Windows;

namespace GitLurker.UI.ViewModels
{
    public class SettingsViewModel : Screen
    {
        #region Fields

        private List<CurrentOperation> _operations = new List<CurrentOperation>
        {
            CurrentOperation.Merge,
            CurrentOperation.Rebase
        };

        private CurrentOperation _selectedOperation;
        private SettingsFile _settingsFile;
        private GameSettingsFile _gameSettingsFile;
        private WindowsLink _windowsStartupService;
        private FlyoutService _flyoutService;
        private RepositoryService _repositoryService;
        private PropertyChangedBase _flyoutContent;
        private ThemeService _themeService;
        private bool _steamLoading;
        private bool _epicLoading;
        private bool _flyoutOpen;
        private string _flyoutHeader;
        private int _selectedTabIndex;

        #endregion

        #region Constructors

        public SettingsViewModel(
            FlyoutService flyoutService, 
            SettingsFile settingsFile, 
            ThemeService themeService,
            WindowsLink windowsLink, 
            DialogService dialogService, 
            RepositoryService repositoryService)
        {
            _flyoutService = flyoutService;
            _repositoryService = repositoryService;
            _themeService = themeService;
            _windowsStartupService = windowsLink;
            _settingsFile = settingsFile;
            _settingsFile.Initialize();

            _gameSettingsFile = new GameSettingsFile();
            _gameSettingsFile.Initialize();

            RepoManager = new RepoManagerViewModel(_settingsFile);
            Hotkey = new HotkeyViewModel(_settingsFile.Entity.HotKey, Save);
            DevToysHotkey = new HotkeyViewModel(_settingsFile.Entity.DevToysHotKey, Save, "DevToys");
            ActionManager = new CustomActionManagerViewModel();
            dialogService.Register(this);

            _flyoutService.ShowFlyoutRequested += FlyoutService_ShowFlyout;
            _flyoutService.CloseFlyoutRequested += FlyoutService_CloseFlyout;
            _selectedOperation = _settingsFile.Entity.RebaseOperation;
        }

        #endregion

        #region Properties

        public IEnumerable<CurrentOperation> Operations => _operations;

        public RepoManagerViewModel RepoManager { get; set; }

        public HotkeyViewModel Hotkey { get; set; }

        public HotkeyViewModel DevToysHotkey { get; set; }

        public CustomActionManagerViewModel ActionManager { get; set; }

        public bool HasNugetSource => _settingsFile.HasNugetSource();

        public bool IsAdmin => _settingsFile.Entity.IsAdmin;

        public IEnumerable<Scheme> Schemes => _themeService.GetSchemes();

        public IEnumerable<Scheme> SteamSchemes => _themeService.GetSchemes();

        public bool SteamLoading
        {
            get => _steamLoading;
            set
            {
                _steamLoading = value;
                NotifyOfPropertyChange();
            }
        }

        public bool EpicLoading
        {
            get => _epicLoading;
            set
            {
                _epicLoading = value;
                NotifyOfPropertyChange();
            }
        }

        public CurrentOperation SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                _selectedOperation = value;
                NotifyOfPropertyChange();
            }
        }

        public string FlyoutHeader
        {
            get => _flyoutHeader;
            set
            {
                _flyoutHeader = value;
                NotifyOfPropertyChange(() => FlyoutHeader);
            }
        }

        public Scheme SelectedScheme
        {
            get => _settingsFile.Entity.Scheme;
            set
            {
                if (_settingsFile.Entity.Scheme != value)
                {
                    _themeService.Change(Theme.Dark, value);
                    _settingsFile.Entity.Scheme = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public Scheme SelectedSteamScheme
        {
            get => _gameSettingsFile.Entity.Scheme;
            set
            {
                if (_gameSettingsFile.Entity.Scheme != value)
                {
                    _gameSettingsFile.Entity.Scheme = value;
                    _gameSettingsFile.Save();
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsSteamInitialized => !string.IsNullOrEmpty(_gameSettingsFile.Entity.SteamExePath);

        public bool IsEpicInitialized => !string.IsNullOrEmpty(_gameSettingsFile.Entity.EpicExePath);

        public bool FlyoutOpen
        {
            get =>_flyoutOpen;
            set
            {
                if (!value)
                {
                    _flyoutService.NotifyFlyoutClosed();
                }

                _flyoutOpen = value;
                NotifyOfPropertyChange(() => FlyoutOpen);
            }
        }

        public PropertyChangedBase FlyoutContent
        {
            get => _flyoutContent;
            set
            {
                _flyoutContent = value;
                NotifyOfPropertyChange(() => FlyoutContent);
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                FlyoutOpen = false;
                NotifyOfPropertyChange();
            }
        }

        public bool ConsoleOuput
        {
            get => _settingsFile.Entity.ConsoleOuput;
            set
            {
                _settingsFile.Entity.ConsoleOuput = value;
                NotifyOfPropertyChange();
            }
        }

        public bool StartWithWindows
        {
            get => _settingsFile.Entity.StartWithWindows;
            set
            {
                _settingsFile.Entity.StartWithWindows = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsSteamEnabled
        {
            get => _settingsFile.Entity.SteamEnabled;
            set
            {
                _settingsFile.Entity.SteamEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool AddToStartMenu
        {
            get => _settingsFile.Entity.AddToStartMenu;
            set
            {
                _settingsFile.Entity.AddToStartMenu = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        public DoubleClickCommand RebaseOperationCommand => new DoubleClickCommand((operation) => OnRebaseOperationChanged(operation));

        public void OnRebaseOperationChanged(object operation)
        {
            if(operation is CurrentOperation currentOperation)
            {
                SelectedOperation = currentOperation;
                _settingsFile.Entity.RebaseOperation = currentOperation;
                _settingsFile.Save();
            }
        }

        public void ShowFlyout(string header, PropertyChangedBase content)
        {
            FlyoutHeader = header;
            FlyoutContent = content;
            FlyoutOpen = true;
        }

        public async Task InitializeGames()
        {
            SteamLoading = true;

            var steamService = new SteamService();
            var steamPath = await steamService.InitializeAsync(_gameSettingsFile.Entity.SteamExePath);
            if (!string.IsNullOrEmpty(steamPath))
            {
                _gameSettingsFile.SetSteamExePath(steamPath);
                NotifyOfPropertyChange(() => IsSteamInitialized);
            }

            SteamLoading = false;
            EpicLoading = true;

            var epicService = new EpicService();
            var epicPath = await epicService.InitializeAsync(_gameSettingsFile.Entity.EpicExePath);
            if (!string.IsNullOrEmpty(epicPath))
            {
                _gameSettingsFile.SetEpicExePath(epicPath);
                NotifyOfPropertyChange(() => IsEpicInitialized);
            }

            EpicLoading = false;
        }

        public void CloseFlyout()
        {
            FlyoutOpen = false;

            // We use the field since we dont want to notify the UI
            _flyoutContent = null;
        }

        public void Save()
        {
            _settingsFile.Save();
        }

        public void ToggleAddMenu()
        {
            AddToStartMenu = !AddToStartMenu;
            if (AddToStartMenu)
            {
                _windowsStartupService.AddStartMenu();
            }
            else
            {
                _windowsStartupService.RemoveStartMenu();
            }

            Save();
        }

        public void ToggleStartWithWindows()
        {
            StartWithWindows = !StartWithWindows;
            if (StartWithWindows)
            {
                _windowsStartupService.AddStartWithWindows();
            }
            else
            {
                _windowsStartupService.RemoveStartWithWindows();
            }

            Save();
        }

        public async void ToggleSteam()
        {
            IsSteamEnabled = !IsSteamEnabled;

            _settingsFile.Entity.SteamEnabled = IsSteamEnabled;
            if (IsSteamEnabled)
            {
                await Task.Run(async () => await InitializeGames());
            }

            Save();
        }

        public void ToggleConsoleOutput()
        {
            ConsoleOuput = !ConsoleOuput;
            Save();
        }

        public void ToggleLocalNuget()
        {
            string path = null;
            if (!HasNugetSource)
            {
                using var dialog = new System.Windows.Forms.FolderBrowserDialog();
                var result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                path = dialog.SelectedPath;
            }

            _settingsFile.Entity.NugetSource = path;
            NotifyOfPropertyChange(() => HasNugetSource);
            Save();
        }

        public void ToggleAdmin()
        {
            var isAdmin = !_settingsFile.Entity.IsAdmin;
            

            var gitLurker = _repositoryService.GetRepo("GitLurker");
            if (gitLurker == null)
            {
                return;
            }

            var manifestPath = Path.Combine(gitLurker.Folder, @"src\GitLurker.UI\app.manifest");
            if (File.Exists(manifestPath))
            {
                var xml = XDocument.Parse(File.ReadAllText(manifestPath));
                XNamespace ns = "urn:schemas-microsoft-com:asm.v3";
                var executionLevel = xml.Root.Descendants(ns + "requestedExecutionLevel").FirstOrDefault();
                var level = "asInvoker";
                if (isAdmin)
                {
                    level = "highestAvailable";
                }

                var levelAttribute = executionLevel.Attribute("level");
                if (levelAttribute != null)
                {
                    levelAttribute.SetValue(level);
                }

                File.WriteAllText(manifestPath, xml.ToString());
            }

            _settingsFile.Entity.IsAdmin = isAdmin;
            NotifyOfPropertyChange(() => IsAdmin);
            Save();

            _ = gitLurker.ExecuteCommandAsync("dotnet run --project .\\src\\GitLurker.UI\\GitLurker.UI.csproj -c release");
            Process.GetCurrentProcess().Kill();
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Save();
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private void FlyoutService_ShowFlyout(object _, FlyoutRequest e) => ShowFlyout(e.Header, e.Content);

        private void FlyoutService_CloseFlyout(object _, EventArgs e) => CloseFlyout();

        #endregion
    }
}
