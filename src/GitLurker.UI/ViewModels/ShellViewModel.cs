namespace GitLurker.UI.ViewModels;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Desktop.Robot;
using Desktop.Robot.Extensions;
using GitLurker.Core.Models;
using GitLurker.Core.Services;
using GitLurker.UI.Helpers;
using GitLurker.UI.Messages;
using GitLurker.UI.Services;
using GitLurker.UI.Views;
using Lurker.Patreon;
using Lurker.Windows;
using NHotkey.Wpf;
using TextCopy;

public class ShellViewModel : Screen, IHandle<CloseMessage>, IHandle<PatronMessage>, IHandle<Messages.ActionMessage>, IDisposable
{
    #region Fields

    private static readonly Robot Robot = new();
    private static readonly string DefaultWaterMark = "Search";
    private Window _parent;
    private SettingsFile _settingsFile;
    private ThemeService _themeService;
    private PatreonService _patronService;
    private KeyboardService _keyboardService;
    private MouseService _mouseService;
    private RepositoryService _repositoryService;
    private ConsoleService _consoleService;
    private GithubUpdateManager _updateManager;
    private WindowsLink _startupService;
    private ConsoleViewModel _console;
    private string _searchTerm;
    private string _searchWatermark;
    private bool _isVisible;
    private bool _showInTaskBar;
    private bool _disable;
    private bool _showConsoleOverview;
    private IEventAggregator _eventAggregator;
    private bool _topMost;
    private string _version;
    private double _dpiX = 1;
    private double _dpiY = 1;
    private bool _hasSurfaceDial;
    private bool _isConsoleOpen;
    private bool _needUpdate;
    private bool _updating;
    private string _consoleHeader;
    private IDebounceService _debouncer;
    private IDebounceService _searchDebouncer;
    private WorkspaceViewModel _workspaceViewModel;
    private GameLibraryViewModel _gameLibraryViewModel;
    private AudioLibraryViewModel _audioLibraryViewModel;
    private SettingsViewModel _settingsViewModel;
    private ModeStatusViewModel _modeStatusViewModel;
    private IItemListViewModel _activeAction;

    #endregion

    #region Constructors

    public ShellViewModel(
        IEventAggregator aggregator,
        IDebounceService debouncer,
        IDebounceService searchDebouncer,
        SettingsFile settings,
        KeyboardService keyboardService,
        MouseService mouseService,
        WindowsLink startupService,
        RepositoryService repositoryService,
        ThemeService themeService,
        ConsoleService consoleService,
        GithubUpdateManager updateManager,
        ConsoleViewModel console,
        PatreonService patronService,
        SettingsViewModel settingsViewModel,
        ModeStatusViewModel modeStatusViewModel)
    {
        _console = console;
        _searchTerm = string.Empty;
        _searchWatermark = DefaultWaterMark;
        _isVisible = false;
        _showInTaskBar = true;
        _eventAggregator = aggregator;
        _keyboardService = keyboardService;
        _mouseService = mouseService;
        _patronService = patronService;
        _startupService = startupService;
        _repositoryService = repositoryService;
        _consoleService = consoleService;
        _settingsFile = settings;
        _updateManager = updateManager;
        _debouncer = debouncer;
        _searchDebouncer = searchDebouncer;
        _themeService = themeService;
        _modeStatusViewModel = modeStatusViewModel;

        _updateManager.UpdateRequested += UpdateManager_UpdateRequested;
        _settingsFile.OnFileSaved += OnSettingsSave;
        _settingsFile.Initialize();

        _consoleService.ShowRequested += ConsoleService_ShowRequested;
        _console.OnExecute += Console_OnExecute;

        ApplySettings(settings);

        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        _version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

        _keyboardService.EscapePressed += KeyboardService_EscapePressed;
        _keyboardService.EnterPressed += KeyboardService_EnterPressed;
        _keyboardService.DownPressed += KeyboardService_DownPressed;
        _keyboardService.UpPressed += KeyboardService_UpPressed;
        _keyboardService.NextTabPressed += KeyboardService_NextTabPressed;
        _keyboardService.EnterLongPressed += KeyboardService_EnterLongPressed;

        _workspaceViewModel = new WorkspaceViewModel(_repositoryService, _consoleService, _keyboardService);
        _gameLibraryViewModel = new GameLibraryViewModel();
        _audioLibraryViewModel = new AudioLibraryViewModel(_mouseService);

        _workspaceViewModel.ShowRecent();
        if (settings.Entity.SteamEnabled)
        {
            _gameLibraryViewModel.ShowRecent();
        }

        SetMode();
        RefreshItems();

        SetGlobalHotkey();
        _eventAggregator.SubscribeOnPublishedThread(this);
        _settingsViewModel = settingsViewModel;
    }

    #endregion

    #region Properties

    public ModeStatusViewModel ModeStatusViewModel => _modeStatusViewModel;

    public bool IsNotPledged => !_patronService.IsPledged && !NeedUpdate;

    public DoubleClickCommand ShowSettings => new(OpenSettings);

    public IItemListViewModel ItemListViewModel { get; private set; }

    public ConsoleViewModel Console => _console;

    public bool ShowConsoleOutput => _settingsFile.Entity.ConsoleOuput && UpToDate;

    public bool UpToDate => !NeedUpdate;

    public string Version => _version;

    public bool Updating
    {
        get
        {
            return _updating;
        }

        set
        {
            _updating = value;
            NotifyOfPropertyChange();
        }
    }

    public bool ShowConsoleOverview
    {
        get
        {
            return _showConsoleOverview;
        }

        set
        {
            _showConsoleOverview = value;
            NotifyOfPropertyChange();
        }
    }

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
            _searchDebouncer.Debounce(250, () => Search(_searchTerm));
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
            NotifyOfPropertyChange(() => IsNotPledged);
        }
    }

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

    protected ShellView View { get; private set; }

    private bool GameMode => ItemListViewModel is GameLibraryViewModel;

    #endregion

    #region Methods

    public async void OpenSettings(object parameter)
    {
        if (_settingsViewModel.IsActive)
        {
            return;
        }

        await IoC.Get<IWindowManager>().ShowWindowAsync(_settingsViewModel);
    }

    public void Search(string term)
    {
        ItemListViewModel?.Search(term);
    }

    public void OpenConsole() => IsConsoleOpen = true;

    public Task HandleAsync(CloseMessage message, CancellationToken cancellationToken)
    {
        IsVisible = false;
        return Task.CompletedTask;
    }

    public void HideWindow()
    {
        if (ItemListViewModel == null || ItemListViewModel.Close())
        {
            IsConsoleOpen = false;
            TopMost = false;
            IsVisible = false;
            SearchTerm = string.Empty;
            ItemListViewModel?.Clear();
        }
    }

    public void OpenSettings() => OpenSettings(null);

    public async void RefreshItems()
    {
        Disable = true;
        SearchTerm = string.Empty;
        SearchWatermark = "Loading...";

        ItemListViewModel.Clear();
        await ItemListViewModel.RefreshItems();
        var gitLurkerRepo = _repositoryService.GetAllRepo().FirstOrDefault(r => r.Name == "GitLurker");
        if (gitLurkerRepo != null && AppDomain.CurrentDomain.BaseDirectory.StartsWith(gitLurkerRepo.Folder, StringComparison.InvariantCultureIgnoreCase))
        {
            _updateManager.Watch(gitLurkerRepo);
        }

        Disable = false;
        SearchWatermark = DefaultWaterMark;

        FocusSearch();
        ItemListViewModel.ShowRecent();
    }

    public async void Update()
    {
        if (Updating)
        {
            return;
        }

        NeedUpdate = false;
        Updating = true;

        await _updateManager.Update();

        Execute.OnUIThread(async () =>
        {
            _parent.Close();
            await TryCloseAsync();

            Process.GetCurrentProcess().Close();
        });
    }

    public async void Close()
    {
        await _settingsViewModel.TryCloseAsync();

        _parent.Close();
        await TryCloseAsync();
    }

    public async void OpenPatreon()
    {
        var viewModel = IoC.Get<PatreonViewModel>();
        if (viewModel.IsActive)
        {
            return;
        }

        await IoC.Get<IWindowManager>().ShowWindowAsync(viewModel);
    }

    public Task HandleAsync(PatronMessage message, CancellationToken cancellationToken)
    {
        NotifyOfPropertyChange(() => IsNotPledged);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _console.OnExecute -= Console_OnExecute;
            _consoleService.ShowRequested -= ConsoleService_ShowRequested;

            _console?.Dispose();

            _keyboardService.EscapePressed -= KeyboardService_EscapePressed;
            _keyboardService.EnterPressed -= KeyboardService_EnterPressed;
            _keyboardService.DownPressed -= KeyboardService_DownPressed;
            _keyboardService.UpPressed -= KeyboardService_UpPressed;
            _keyboardService.NextTabPressed -= KeyboardService_NextTabPressed;
            _keyboardService.EnterLongPressed -= KeyboardService_EnterLongPressed;
            _keyboardService?.Dispose();
            _gameLibraryViewModel?.Dispose();
        }
    }

    protected override async void OnViewLoaded(object view)
    {
        View = view as ShellView;

        var source = PresentationSource.FromVisual(View);
        if (source != null)
        {
            _dpiX = source.CompositionTarget.TransformToDevice.M11;
            _dpiY = source.CompositionTarget.TransformToDevice.M22;
        }

        await Task.Delay(200);
        await _keyboardService.InstallAsync();
        await _mouseService.InstallAsync();

        // Needs to be done after Winook
        ShowInTaskBar = false;
        HideFromAltTab(View);
    }

    private static void SetHotkey(Hotkey hotkey, System.Action callback, string name)
    {
        if (!hotkey.IsDefined())
        {
            return;
        }

        var modifier = Enum.Parse<System.Windows.Input.ModifierKeys>(hotkey.Modifier.ToString());

        if (Enum.TryParse(hotkey.KeyCode.ToString(), ignoreCase: true, out System.Windows.Input.Key key))
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(name, key, modifier, (s, e) => callback());
            }
            catch (NHotkey.HotkeyAlreadyRegisteredException)
            {
            }
        }
    }

    private void SetMode()
    {
        switch (_settingsFile.Entity.Mode)
        {
            case Mode.Game:
                var steamSettings = new GameSettingsFile();
                steamSettings.Initialize();
                _themeService.Apply(steamSettings.Entity.Scheme);
                ItemListViewModel = _gameLibraryViewModel;
                break;
            case Mode.Git:
                ItemListViewModel = _workspaceViewModel;
                _themeService.Apply();
                break;
            case Mode.Audio:
                ItemListViewModel = _audioLibraryViewModel;
                break;
        }
    }

    private async void Console_OnExecute(object sender, bool execute)
    {
        if (!execute && _console.Lines.Any())
        {
            await Task.Delay(1800);
        }

        ShowConsoleOverview = execute;
    }

    private void UpdateManager_UpdateRequested(object sender, EventArgs e) => NeedUpdate = true;

    private void ConsoleService_ShowRequested(object sender, EventArgs e) => OpenConsole();

    private async void OpenDevtoys()
    {
        await new ProcessService("").ExecuteCommandAsync("start devtoys:");
    }

    public void SetGit()
    {
        _themeService.Apply();
        ItemListViewModel = _workspaceViewModel;
        _settingsFile.Entity.Mode = Mode.Git;

        _settingsFile.Save();
        ItemListViewModel.ShowRecent();
        NotifyOfPropertyChange(() => ItemListViewModel);
    }

    public void SetAudio()
    {
        _themeService.Apply();
        ItemListViewModel = _audioLibraryViewModel;
        _settingsFile.Entity.Mode = Mode.Audio;

        _settingsFile.Save();
        ItemListViewModel.ShowRecent();
        NotifyOfPropertyChange(() => ItemListViewModel);
    }

    public void SetGame()
    {
        var steamSettings = new GameSettingsFile();
        steamSettings.Initialize();
        _themeService.Apply(steamSettings.Entity.Scheme);
        ItemListViewModel = _gameLibraryViewModel;
        _settingsFile.Entity.Mode = Mode.Game;

        _settingsFile.Save();
        ItemListViewModel.ShowRecent();
        NotifyOfPropertyChange(() => ItemListViewModel);
    }

    private void ToggleWindow()
    {
        if (_debouncer.HasTimer)
        {
            _debouncer.Reset();
            SearchTerm = string.Empty;

            var nextMode = _settingsFile.GetNextMode();

            switch (nextMode)
            {
                case Mode.Git:
                    _themeService.Apply();
                    ItemListViewModel = _workspaceViewModel;
                    _settingsFile.Entity.Mode = Mode.Git;
                    break;
                case Mode.Audio:
                    _themeService.Apply();
                    ItemListViewModel = _audioLibraryViewModel;
                    _settingsFile.Entity.Mode = Mode.Audio;
                    break;
                case Mode.Game:
                    var steamSettings = new GameSettingsFile();
                    steamSettings.Initialize();
                    _themeService.Apply(steamSettings.Entity.Scheme);
                    ItemListViewModel = _gameLibraryViewModel;
                    _settingsFile.Entity.Mode = Mode.Game;
                    break;
            }

            _settingsFile.Save();
            ItemListViewModel.ShowRecent();
            NotifyOfPropertyChange(() => ItemListViewModel);

            if (!_isVisible)
            {
                ShowWindow();
            }
        }
        else
        {
            _debouncer.Debounce(175, () =>
            {
                if (IsVisible)
                {
                    HideWindow();
                    return;
                }

                ShowWindow();
            });
        }
    }

    private void ShowWindow()
    {
        ItemListViewModel?.ShowRecent();

        TopMost = true;
        IsVisible = true;
        HandleScreenPosition();
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

    private void OnSettingsSave(object sender, Settings e)
    {
        SetGlobalHotkey();

        NotifyOfPropertyChange(() => ShowConsoleOutput);
    }

    private void ShowGit()
        => Show(_workspaceViewModel);

    private void ShowGame()
        => Show(_gameLibraryViewModel);

    private void Show(IItemListViewModel itemList)
    {
        ItemListViewModel = itemList;
        ItemListViewModel.ShowRecent();
        NotifyOfPropertyChange(() => ItemListViewModel);
    }

    private void SetGlobalHotkey()
    {
        var settings = new SettingsFile();
        settings.Initialize();

        SetHotkey(settings.Entity.HotKey, ToggleWindow, "Open");
        SetHotkey(settings.Entity.DevToysHotKey, OpenDevtoys, "OpenDevToys");

        foreach (var snippet in settings.Entity.Snippets)
        {
            SetHotkey(snippet.Hotkey, () => TypeSnippet(snippet.Value), snippet.Id.ToString());
        }
    }

    private async void TypeSnippet(string value)
    {
        await ClipboardService.SetTextAsync(value);
        Robot.CombineKeys(Key.Control, Key.V);
    }

    private void ApplySettings(SettingsFile settings)
    {
        if (settings.Entity.AddToStartMenu)
        {
            _startupService.AddStartMenu();
        }
        else
        {
            _startupService.RemoveStartMenu();
        }

        if (settings.Entity.StartWithWindows)
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
            var gitLurkerHeight = View.Height * _dpiY;
            var gitLurkerWidth = View.Width * _dpiX;

            var top = (primaryScreen.Bounds.Height / 2) - (gitLurkerHeight / 2);
            var left = (primaryScreen.Bounds.Width / 2) - (gitLurkerWidth / 2);

            top /= _dpiY;
            left /= _dpiX;

            try
            {
                if (View.Top != top)
                {
                    View.Top = top;
                }

                if (View.Left != left)
                {
                    View.Left = left;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Thread failure! Cannot move window!: {ex.Message}");
            }
        });
    }

    private void KeyboardService_EscapePressed(object sender, EventArgs e)
    {
        if (_activeAction != null)
        {
            ClearAction();

            return;
        }

        HideWindow();
    }

    private async void KeyboardService_EnterPressed(object sender, EventArgs e)
    {
        if (await ItemListViewModel.Open(false))
        {
            HideWindow();
        }
        else
        {
            SearchTerm = string.Empty;

            if (_activeAction != null)
            {
                ClearAction();
            }
        }
    }

    private void ClearAction()
    {
        _activeAction = null;
        SearchWatermark = DefaultWaterMark;
        ItemListViewModel = _workspaceViewModel;
        ItemListViewModel.ShowRecent();

        NotifyOfPropertyChange(() => ItemListViewModel);
    }

    private void KeyboardService_DownPressed(object sender, EventArgs e)
        => ItemListViewModel.MoveDown();

    private void KeyboardService_UpPressed(object sender, EventArgs e)
        => ItemListViewModel.MoveUp();

    private void KeyboardService_NextTabPressed(object sender, EventArgs e)
        => ItemListViewModel.NextTabPressed();

    private void KeyboardService_EnterLongPressed(object sender, EventArgs e)
        => ItemListViewModel.EnterLongPressed();

    public Task HandleAsync(Messages.ActionMessage message, CancellationToken cancellationToken)
    {
        _activeAction = message.ListViewModel;
        SearchWatermark = message.WaterMark;
        ItemListViewModel = _activeAction;
        ItemListViewModel.ShowRecent();

        if (message.Focus)
        {
            FocusSearch();
        }

        NotifyOfPropertyChange(() => ItemListViewModel);

        return Task.CompletedTask;
    }

    #endregion
}
