namespace GitLurker.UI.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Core.Models;
using Lurker.BattleNet.Models;
using Lurker.BattleNet.Services;
using Lurker.Common.Models;
using Lurker.Epic.Models;
using Lurker.Epic.Services;
using Lurker.Steam.Models;
using Lurker.Steam.Services;

public class GameLibraryViewModel : PropertyChangedBase, IItemListViewModel, IDisposable
{
    private BattleNetService _battleNetService;
    private SteamService _steamService;
    private EpicService _epicService;
    private List<BattleNetGame> _battleNetGames;
    private List<SteamGame> _steamGames;
    private List<EpicGame> _epicGames;
    private ObservableCollection<GameViewModel> _gameViewModels;
    private GameViewModel _selectedGameViewModel;
    private bool _mouseOver;
    private bool _initialize;

    public GameLibraryViewModel()
    {
        _epicGames = new List<EpicGame>();
        _steamGames = new List<SteamGame>();
        _battleNetGames = new List<BattleNetGame>();
        _battleNetService = new BattleNetService();
        _steamService = new SteamService();
        _epicService = new EpicService();
        _gameViewModels = new ObservableCollection<GameViewModel>();
        _gameViewModels.CollectionChanged += GameViewModels_CollectionChanged;
    }

    #region Properties

    public bool NoGamesDisplayed => !_gameViewModels.Any();

    public ObservableCollection<GameViewModel> GameViewModels => _gameViewModels;

    public GameViewModel SelectedGameViewModel
    {
        get
        {
            return _selectedGameViewModel;
        }

        private set
        {
            _selectedGameViewModel = value;
            NotifyOfPropertyChange(() => HasSelectedGame);
        }
    }

    public bool HasSelectedGame => SelectedGameViewModel != null || _mouseOver;

    private IEnumerable<GameBase> Games => _steamGames.Cast<GameBase>().Concat(_epicGames).Concat(_battleNetGames);

    #endregion

    #region Methods

    public void Clear()
    {
        SelectedGameViewModel = null;
        Execute.OnUIThread(() => GameViewModels.Clear());
    }

    public bool Close()
    {
        Clear();
        return true;
    }

    public void EnterLongPressed()
    {
        return;
    }

    public void MoveUp()
    {
        if (SelectedGameViewModel == null)
        {
            return;
        }

        SelectedGameViewModel = _gameViewModels.FirstOrDefault(g => g.Id == SelectedGameViewModel.Id);
        var index = _gameViewModels.IndexOf(SelectedGameViewModel);
        if (index <= 0)
        {
            return;
        }

        index--;
        SelectedGameViewModel.IsSelected = false;
        SelectedGameViewModel = _gameViewModels.ElementAt(index);
        SelectedGameViewModel.IsSelected = true;
    }

    public void MoveDown()
    {
        if (SelectedGameViewModel == null)
        {
            SelectedGameViewModel = _gameViewModels.FirstOrDefault();
            if (SelectedGameViewModel != null)
            {
                SelectedGameViewModel.IsSelected = true;
            }

            return;
        }

        SelectedGameViewModel = _gameViewModels.FirstOrDefault(g => g.Id == SelectedGameViewModel.Id);

        var index = _gameViewModels.IndexOf(SelectedGameViewModel);
        if (index == -1 || (index + 1) >= _gameViewModels.Count)
        {
            return;
        }

        index++;
        SelectedGameViewModel.IsSelected = false;
        SelectedGameViewModel = _gameViewModels.ElementAt(index);
        SelectedGameViewModel.IsSelected = true;
    }

    public void NextTabPressed()
    {
        return;
    }

    public Task<bool> Open(bool skipModifier)
    {
        ExecuteOnGame((g) => g.Open());

        return Task.FromResult(true);
    }

    public Task RefreshItems()
        => Task.Run(async () =>
        {
            var settings = new GameSettingsFile();
            settings.Initialize();

            await CheckInitialize(settings);
            _battleNetGames = _battleNetService.FindGames();
            _steamGames = _steamService.FindGames();
            _epicGames = _epicService.FindGames();
        });

    public void Search(string term)
    {
        Clear();

        if (string.IsNullOrEmpty(term))
        {
            ShowRecent();
            return;
        }

        var startWith = Games.Where(r => r.Name.ToUpper().StartsWith(term.ToUpper()));
        var contain = Games.Where(r => r.Name.ToUpper().Contains(term.ToUpper())).ToList();
        contain.InsertRange(0, startWith);

        var matches = contain.Distinct().Take(5);

        foreach (var game in matches)
        {
            GameViewModels.Add(new GameViewModel(game));
        }
    }

    public async void ShowRecent()
    {
        Clear();
        var settings = new GameSettingsFile();
        settings.Initialize();

        await CheckInitialize(settings);

        foreach (var gameId in settings.Entity.RecentGameIds)
        {
            var game = Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null)
            {
                continue;
            }

            Execute.OnUIThread(() => _gameViewModels.Add(new GameViewModel(game)));
        }
    }

    public void OnMouseEnter()
    {
        _mouseOver = true;
        NotifyOfPropertyChange(() => HasSelectedGame);
    }

    public void OnMouseLeave()
    {
        _mouseOver = false;
        NotifyOfPropertyChange(() => HasSelectedGame);
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
            _gameViewModels.CollectionChanged -= GameViewModels_CollectionChanged;
        }
    }

    private void ExecuteOnGame(System.Action<GameViewModel> action)
    {
        if (SelectedGameViewModel != null)
        {
            action(SelectedGameViewModel);
            return;
        }

        var firstGame = _gameViewModels.FirstOrDefault();
        if (firstGame != null)
        {
            action(firstGame);
        }
    }

    private async Task CheckInitialize(GameSettingsFile settings)
    {
        if (!_initialize)
        {
            var steamPath = settings.Entity.SteamExePath;
            if (!settings.Entity.SteamAsked || !string.IsNullOrEmpty(steamPath))
            {
                steamPath = await _steamService.InitializeAsync(steamPath);
                settings.SetSteamExePath(steamPath);
                _steamGames = _steamService.FindGames();
            }

            var epicPath = settings.Entity.EpicExePath;
            if (!settings.Entity.EpicAsked || !string.IsNullOrEmpty(epicPath))
            {
                epicPath = await _epicService.InitializeAsync(settings.Entity.EpicExePath);
                settings.SetEpicExePath(epicPath);
                _epicGames = _epicService.FindGames();
            }

            _battleNetGames = _battleNetService.FindGames();
            settings.Entity.BattleNetInstalled = _battleNetGames.Any();
            settings.Save();

            _initialize = true;
        }
    }

    private void GameViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NotifyOfPropertyChange(nameof(NoGamesDisplayed));
    }

    #endregion
}
