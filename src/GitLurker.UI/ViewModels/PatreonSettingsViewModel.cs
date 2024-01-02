namespace GitLurker.UI.ViewModels;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.Core.Services;
using GitLurker.UI.Messages;
using GitLurker.UI.Services;
using Lurker.BattleNet.Services;
using Lurker.Epic.Services;
using Lurker.Steam.Services;

public class PatreonSettingsViewModel : PropertyChangedBase, IHandle<PatronMessage>
{
    #region Fields

    private bool _steamLoading;
    private bool _epicLoading;
    private bool _battleNetLoading;
    private ThemeService _themeService;
    private SettingsFile _settingsFile;
    private GameSettingsFile _gameSettingsFile;
    private PatronService _patronService;
    private IEventAggregator _eventAggregator;

    #endregion

    #region Constructors

    public PatreonSettingsViewModel(SettingsFile settingsFile, ThemeService themeService, PatronService patronService, IEventAggregator eventAggregator)
    {
        _themeService = themeService;
        _patronService = patronService;
        _eventAggregator = eventAggregator;
        _settingsFile = settingsFile;
        _settingsFile.Initialize();

        _gameSettingsFile = new GameSettingsFile();
        _gameSettingsFile.Initialize();
        _eventAggregator.SubscribeOnPublishedThread(this);

        ActionManager = new CustomActionManagerViewModel();
    }

    #endregion

    #region Properties

    public CustomActionManagerViewModel ActionManager { get; set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(_patronService.PatreonId);

    public bool IsNotLoggedIn => !IsLoggedIn;

    public bool IsPledged => _patronService.IsPledged;

    public bool IsNotPledged => !IsPledged;

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

    public bool BattleNetLoading
    {
        get => _battleNetLoading;
        set
        {
            _battleNetLoading = value;
            NotifyOfPropertyChange();
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

    public IEnumerable<Scheme> Schemes => _themeService.GetSchemes();

    public IEnumerable<Scheme> SteamSchemes => _themeService.GetSchemes();

    public bool IsSteamInitialized => !string.IsNullOrEmpty(_gameSettingsFile.Entity.SteamExePath);

    public bool IsEpicInitialized => !string.IsNullOrEmpty(_gameSettingsFile.Entity.EpicExePath);

    public bool IsBattleNetInitialized => _gameSettingsFile.Entity.BattleNetInstalled;

    public bool IsSteamEnabled
    {
        get => _settingsFile.Entity.SteamEnabled;
        set
        {
            _settingsFile.Entity.SteamEnabled = value;
            NotifyOfPropertyChange();
        }
    }

    #endregion

    #region Methods

    public async void Logout()
    {
        _patronService.LogOut();
        await _eventAggregator.PublishOnCurrentThreadAsync(new PatronMessage());
    }

    public Task HandleAsync(PatronMessage message, CancellationToken cancellationToken)
    {
        Notify();

        return Task.CompletedTask;
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

        BattleNetLoading = true;

        var battleNetService = new BattleNetService();
        _gameSettingsFile.Entity.BattleNetInstalled = battleNetService.FindGames().Any();
        _gameSettingsFile.Save();

        BattleNetLoading = false;
    }

    public async void ToggleGames()
    {
        IsSteamEnabled = !IsSteamEnabled;

        _settingsFile.Entity.SteamEnabled = IsSteamEnabled;
        if (IsSteamEnabled)
        {
            await Task.Run(async () => await InitializeGames());
        }

        _settingsFile.Save();
    }

    private void Notify()
    {
        NotifyOfPropertyChange(() => IsLoggedIn);
        NotifyOfPropertyChange(() => IsNotLoggedIn);
        NotifyOfPropertyChange(() => IsPledged);
        NotifyOfPropertyChange(() => IsNotPledged);
    }

    #endregion
}
