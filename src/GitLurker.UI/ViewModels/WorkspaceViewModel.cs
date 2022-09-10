namespace GitLurker.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;
    using GitLurker.UI.Services;

    public class WorkspaceViewModel: PropertyChangedBase
    {
        #region Fields

        private KeyboardService _keyboardService;
        private RepositoryService _repositoryService;
        private ConsoleService _consoleService;
        private ObservableCollection<RepositoryViewModel> _repos;
        private RepositoryViewModel _selectedRepo;
        private string _lastSearchTerm;
        private bool _mouseOver;

        #endregion

        #region Constructors

        public WorkspaceViewModel(KeyboardService keyboardService, RepositoryService repositoryService, ConsoleService consoleService)
        {
            _repos = new ObservableCollection<RepositoryViewModel>();
            _keyboardService = keyboardService;
            _repositoryService = repositoryService;
            _consoleService = consoleService;

            _keyboardService.DownPressed += KeyboardService_DownPressed;
            _keyboardService.UpPressed += KeyboardService_UpPressed;
            _keyboardService.NextTabPressed += KeyboardService_NextTabPressed;
            _keyboardService.EnterLongPressed += KeyboardService_EnterLongPressed;
            _keyboardService.EnterPressed += KeyboardService_EnterPressed;
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

        public async Task Open(bool skipModifier)
        {
            if (Uri.TryCreate(_lastSearchTerm, UriKind.Absolute, out var result))
            {
                await CloneAsync(result);

                return;
            }

            ExecuteOnRepo((r) => r.Open(skipModifier));
        }

        public async Task CloneAsync(Uri url)
        {
            var workspace = _repositoryService.Workspaces.FirstOrDefault();
            if (workspace == null)
            {
                return;
            }

            _consoleService.Listen(workspace);
            var newRepo = await workspace.CloneAsync(url);
            if (newRepo != null)
            {
                workspace.AddRepo(newRepo);
                _repos.Insert(0, new RepositoryViewModel(newRepo));
            }

            return;
        }

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
                ShowRecent();
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
            Clear();
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
            }

            if (SelectedRepo == null)
            {
                return;
            }

            SelectedRepo.Select();

            if (SelectedRepo.IsBranchManagerOpen)
            {
                SelectedRepo.OpenNewBranch();
            }
            else
            {
                SelectedRepo.ShowBranches(false);
            }
        }

        private async void KeyboardService_EnterPressed(object sender, EventArgs e)
            => await Open(false);

        private void KeyboardService_EnterLongPressed(object sender, EventArgs e)
            => OpenPullRequest();

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
