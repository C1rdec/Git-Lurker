using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Models;
using Lurker.Steam.Models;
using Lurker.Steam.Services;

namespace GitLurker.UI.ViewModels
{
    public class SteamLibraryViewModel : PropertyChangedBase, IItemListViewModel
    {
        private SteamService _steamService;
        private List<SteamGame> _games;
        private ObservableCollection<SteamGameViewModel> _gameViewModels;
        private SteamGameViewModel _selectedGameViewModel;
        private bool _mouseOver;
        private bool _initialize;

        public SteamLibraryViewModel()
        {
            _games = new List<SteamGame>();
            _steamService = new SteamService();
            _gameViewModels = new ObservableCollection<SteamGameViewModel>();
        }

        #region Properties

        public ObservableCollection<SteamGameViewModel> GameViewModels => _gameViewModels;

        public SteamGameViewModel SelectedGameViewModel
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

        public Task Open(bool skipModifier)
        {
            ExecuteOnGame((g) => g.Open());
            return Task.CompletedTask;
        }

        public Task RefreshItems()
            => Task.Run(() => _games = _steamService.FindGames());

        public void Search(string term)
        {
            Clear();

            if (string.IsNullOrEmpty(term))
            {
                ShowRecent();
                return;
            }

            var startWith = _games.Where(r => r.Name.ToUpper().StartsWith(term.ToUpper()));
            var contain = _games.Where(r => r.Name.ToUpper().Contains(term.ToUpper())).ToList();
            contain.InsertRange(0, startWith);

            var matches = contain.Distinct().Take(5);

            foreach (var game in matches)
            {
                GameViewModels.Add(new SteamGameViewModel(game));
            }
        }

        public async void ShowRecent()
        {
            Clear();
            var settings = new SteamSettingsFile();
            settings.Initialize();

            if (!_initialize)
            {
                var path = await _steamService.InitializeAsync(settings.Entity.SteamExePath);

                settings.SetSteamExePath(path);
                _games = _steamService.FindGames();
                _initialize = true;
            }

            foreach (var gameId in settings.Entity.RecentGameIds)
            {
                var game = _games.FirstOrDefault(g => g.Id == gameId);
                if (game == null)
                {
                    continue;
                }

                Execute.OnUIThread(() => _gameViewModels.Add(new SteamGameViewModel(game)));
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

        private void ExecuteOnGame(System.Action<SteamGameViewModel> action)
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

        #endregion
    }
}
