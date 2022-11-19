using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using SteamLurker.Models;
using SteamLurker.Services;

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
                NotifyOfPropertyChange(() => HasSelectedRepo);
            }
        }

        public bool HasSelectedRepo => SelectedGameViewModel != null || _mouseOver;


        #endregion

        public void Clear()
            => Execute.OnUIThread(() => GameViewModels.Clear());

        public bool Close()
        {
            Clear();
            return true;
        }

        public void MoveDown()
        {
            return;

        }

        public void MoveUp()
        {
            return;

        }

        public async Task Open(bool skipModifier)
        {
            ExecuteOnGame((g) => g.Open());
        }

        public void OpenPullRequest()
        {
            return;
        }

        public Task RefreshItems()
        {
            return Task.CompletedTask;
        }

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
            if (!_initialize)
            {
                await _steamService.InitializeAsync();
                _initialize = true;
            }

            _games = _steamService.FindGames();
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
    }
}
