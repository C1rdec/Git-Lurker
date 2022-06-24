namespace GitLurker.UI.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;

    public class WorkspaceViewModel: PropertyChangedBase
    {
        #region Fields

        private KeyboardService _keyboardService;
        private RepositoryService _repositoryService;
        private ObservableCollection<RepositoryViewModel> _repos;
        private RepositoryViewModel _selectedRepo;
        private string _lastSearchTerm;
        private bool _mouseOver;

        #endregion

        #region Constructors

        public WorkspaceViewModel(KeyboardService keyboardService, RepositoryService repositoryService)
        {
            _repos = new ObservableCollection<RepositoryViewModel>();
            _keyboardService = keyboardService;
            _repositoryService = repositoryService;
            _keyboardService.DownPressed += KeyboardService_DownPressed;
            _keyboardService.UpPressed += KeyboardService_UpPressed;
            _keyboardService.NextTabPressed += KeyboardService_NextTabPressed;

            _keyboardService.PreviousTabPressed += keyboardService_PreviousTabPressed;
        }

        #endregion

        #region Properties

        public ObservableCollection<RepositoryViewModel> Repos => _repos;

        public RepositoryViewModel SelectedRepo
        {
            get
            {
                return _selectedRepo;
            }

            private set
            {
                _selectedRepo = value;
                NotifyOfPropertyChange(() => HasSelectedRepo);
            }
        }

        public bool HasSelectedRepo => SelectedRepo != null || !string.IsNullOrEmpty(_lastSearchTerm) || _mouseOver;

        #endregion

        #region Methods

        public void Open() => Open(false);

        public void Open(bool skipModifier)
            => ExecuteOnRepo((r) => r.Open(skipModifier));

        public void OpenPullRequest()
            => ExecuteOnRepo((r) => r.OpenPullRequest());

        public void MoveUp()
        {
            if (SelectedRepo == null)
            {
                return;
            }

            var index = _repos.IndexOf(SelectedRepo);
            if (index <= 0)
            {
                return;
            }

            index--;
            SelectedRepo.UnSelect();
            SelectedRepo = Repos.ElementAt(index);
            SelectedRepo.Select();
        }

        public void MoveDown()
            => MoveDown(false);

        public void MoveDown(bool selectFirst)
        {
            if (SelectedRepo == null)
            {
                SelectedRepo = _repos.FirstOrDefault();
                SelectedRepo?.Select();
                return;
            }

            var index = _repos.IndexOf(_selectedRepo);
            if (index == -1 || (index + 1) >= _repos.Count)
            {
                if (selectFirst)
                {
                    SelectedRepo.UnSelect();
                    SelectedRepo = _repos.FirstOrDefault();
                    SelectedRepo.Select();
                }

                return;
            }

            index++;
            SelectedRepo.UnSelect();
            SelectedRepo = Repos.ElementAt(index);
            SelectedRepo.Select();
        }

        public void OnMouseEnter()
        {
            _mouseOver = true;
            NotifyOfPropertyChange(() => HasSelectedRepo);
        }

        public void OnMouseLeave()
        {
            _mouseOver = false;
            NotifyOfPropertyChange(() => HasSelectedRepo);
        }

        public void Search(string term)
        {
            _lastSearchTerm = term;
            Clear();

            if (string.IsNullOrEmpty(term))
            {
                return;
            }

            var repos = _repositoryService.Search(term);
            foreach (var repo in repos)
            {
                Repos.Add(new RepositoryViewModel(repo));
            }
        }

        public void Clear()
        {
            SelectedRepo = null;
            _repos.Clear();
        }

        public bool Close()
        {
            if (SelectedRepo != null && SelectedRepo.IsBranchManagerOpen)
            {
                SelectedRepo.IsBranchManagerOpen = false;
                return false;
            }

            Clear();
            return true;
        }

        public void ShowRecent()
        {
            var file = new SettingsFile();
            file.Initialize();

            foreach (var folder in file.Entity.RecentRepos)
            {
                var repo = _repositoryService.GetReposiotry(folder);
                if (repo != null)
                {
                    Repos.Add(new RepositoryViewModel(repo));
                }
            }
        }

        public async Task RefreshRepositories()
        {
            await Task.Run(() => _repositoryService.GetWorkspaces());
            _repos.Clear();
        }

        private void KeyboardService_DownPressed(object sender, System.EventArgs e)
        {
            if (SelectedRepo != null && SelectedRepo.IsBranchManagerOpen)
            {
                SelectedRepo.SelectNextBranch();
                return;
            }

            MoveDown();
        }

        private void KeyboardService_UpPressed(object sender, System.EventArgs e) 
        {
            if (SelectedRepo != null && SelectedRepo.IsBranchManagerOpen)
            {
                SelectedRepo.SelectPreviousBranch();
                return;
            }

            MoveUp(); 
        }

        private void KeyboardService_NextTabPressed(object sender, System.EventArgs e) 
        {
            if (SelectedRepo == null)
            {
                SelectedRepo = _repos.FirstOrDefault();
                SelectedRepo.Select();
            }

            SelectedRepo.ToggleBranches();
        }

        private void keyboardService_PreviousTabPressed(object sender, System.EventArgs e)
        {
            ExecuteOnRepo((r) =>
            {
                SelectedRepo = r;
                r.ToggleBranches();
            });
        }

        private void ExecuteOnRepo(System.Action<RepositoryViewModel> action)
        {
            if (SelectedRepo != null)
            {
                action(SelectedRepo);
                return;
            }

            var firstRepo = _repos.FirstOrDefault();
            if (firstRepo != null)
            {
                action(firstRepo);
            }
        }

        #endregion
    }
}
