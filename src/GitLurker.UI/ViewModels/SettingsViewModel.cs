﻿namespace GitLurker.UI.ViewModels;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.Core.Services;
using GitLurker.UI.Services;
using LibGit2Sharp;
using Lurker.Patreon;
using Lurker.Windows;
using Winook;

public class SettingsViewModel : FlyoutScreenBase
{
    #region Fields

    private List<CurrentOperation> _operations =
    [
        CurrentOperation.Merge,
        CurrentOperation.Rebase
    ];

    private CurrentOperation _selectedOperation;
    private SettingsFile _settingsFile;
    private GameSettingsFile _gameSettingsFile;
    private WindowsLink _windowsStartupService;
    private RepositoryService _repositoryService;
    private PatreonService _patronService;
    private int _selectedTabIndex;

    #endregion

    #region Constructors

    public SettingsViewModel(
        FlyoutService flyoutService,
        SettingsFile settingsFile,
        WindowsLink windowsLink,
        DialogService dialogService,
        RepositoryService repositoryService,
        PatreonSettingsViewModel patreonViewModel,
        SnippetManagerViewModel snippetManager,
        NugetSettingsViewModel nugetSettingsViewModel,
        PatreonService patronService)
        : base(flyoutService)
    {
        _repositoryService = repositoryService;
        _windowsStartupService = windowsLink;
        _settingsFile = settingsFile;

        _gameSettingsFile = new GameSettingsFile();
        _gameSettingsFile.Initialize();

        NugetSettings = nugetSettingsViewModel;
        SnippetManager = snippetManager;
        RepoManager = new RepoManagerViewModel(_settingsFile);
        Hotkey = new HotkeyViewModel(_settingsFile.Entity.HotKey, SaveHotKey);
        DevToysHotkey = new HotkeyViewModel(_settingsFile.Entity.DevToysHotKey, SaveDevToy, "DevToys");
        PatreonViewModel = patreonViewModel;

        PatreonViewModel.PropertyChanged += PatreonViewModel_PropertyChanged;

        dialogService.Register(this);
        _selectedOperation = _settingsFile.Entity.RebaseOperation;
        _patronService = patronService;
    }

    #endregion

    #region Properties

    public bool IsNotPledged => !_patronService.IsPledged;

    public NugetSettingsViewModel NugetSettings { get; set; }

    public SnippetManagerViewModel SnippetManager { get; set; }

    public PatreonSettingsViewModel PatreonViewModel { get; set; }

    public IEnumerable<CurrentOperation> Operations => _operations;

    public RepoManagerViewModel RepoManager { get; set; }

    public HotkeyViewModel Hotkey { get; set; }

    public HotkeyViewModel DevToysHotkey { get; set; }

    public bool HasNugetSource => _settingsFile.HasNugetSource();

    public bool IsAdmin => _settingsFile.Entity.IsAdmin;

    public CurrentOperation SelectedOperation
    {
        get => _selectedOperation;
        set
        {
            _selectedOperation = value;
            NotifyOfPropertyChange();
        }
    }

    public bool IsSteamInitialized => !string.IsNullOrEmpty(_gameSettingsFile.Entity.SteamExePath);

    public bool IsEpicInitialized => !string.IsNullOrEmpty(_gameSettingsFile.Entity.EpicExePath);

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

    public bool AudioEnabled
    {
        get => _settingsFile.Entity.AudioEnabled;
        set
        {
            _settingsFile.Entity.AudioEnabled = value;
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

    public DoubleClickCommand RebaseOperationCommand => new((operation) => OnRebaseOperationChanged(operation));

    public void OnRebaseOperationChanged(object operation)
    {
        if (operation is CurrentOperation currentOperation)
        {
            SelectedOperation = currentOperation;
            _settingsFile.Entity.RebaseOperation = currentOperation;
            _settingsFile.Save();
        }
    }

    public void SaveHotKey(KeyCode code, Modifiers modifier)
    {
        _settingsFile.Entity.HotKey.KeyCode = code;
        _settingsFile.Entity.HotKey.Modifier = modifier;

        _settingsFile.Save();
    }

    public void SaveDevToy(KeyCode code, Modifiers modifier)
    {
        _settingsFile.Entity.DevToysHotKey.KeyCode = code;
        _settingsFile.Entity.DevToysHotKey.Modifier = modifier;

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

        _settingsFile.Save();
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

        _settingsFile.Save();
    }

    public void ToggleConsoleOutput()
    {
        ConsoleOuput = !ConsoleOuput;
        _settingsFile.Save();
    }

    public void ToggleAudio()
    {
        AudioEnabled = !AudioEnabled;
        _settingsFile.Save();
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

        _settingsFile.Entity.LocalNugetSource = path;
        NotifyOfPropertyChange(() => HasNugetSource);
        _settingsFile.Save();
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
            levelAttribute?.SetValue(level);

            File.WriteAllText(manifestPath, xml.ToString());
        }

        _settingsFile.Entity.IsAdmin = isAdmin;
        NotifyOfPropertyChange(() => IsAdmin);
        _settingsFile.Save();

        _ = gitLurker.ExecuteCommandAsync("dotnet run --project .\\src\\GitLurker.UI\\GitLurker.UI.csproj -c release");
        Process.GetCurrentProcess().Kill();
    }

    public async void OpenPatreon()
    {
        var viewModel = IoC.Get<PatreonViewModel>();
        if (viewModel.IsActive)
        {
            return;
        }
        await IoC.Get<IWindowManager>().ShowWindowAsync(viewModel);

        await viewModel.ActivateAsync();
    }

    protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        _settingsFile.Save();

        return base.OnDeactivateAsync(close, cancellationToken);
    }

    private void PatreonViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PatreonSettingsViewModel.IsPledged))
        {
            NotifyOfPropertyChange(() => IsNotPledged);
        }
    }

    #endregion
}
