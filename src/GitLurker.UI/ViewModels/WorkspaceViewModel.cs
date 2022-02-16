namespace GitLurker.UI.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using GitLurker.Models;
    using GitLurker.Services;

    public class WorkspaceViewModel
    {
        #region Fields

        private KeyboardService _keyboardService;
        private Workspace _workspace;
        private ObservableCollection<RepositoryViewModel> _repos;
        private RepositoryViewModel _selectedRepo;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceViewModel"/> class.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public WorkspaceViewModel(Workspace workspace, KeyboardService keyboardService)
        {
            _workspace = workspace;
            _repos = new ObservableCollection<RepositoryViewModel>();
            _keyboardService = keyboardService;
            _keyboardService.DownPressed += KeyboardService_DownPressed;
            _keyboardService.UpPressed += KeyboardService_UpPressed;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the repos.
        /// </summary>
        /// <value>The repos.</value>
        public ObservableCollection<RepositoryViewModel> Repos => _repos;

        #endregion

        #region Methods

        public void Search(string term)
        {
            Clear();

            foreach (var repo in _workspace.Search(term))
            {
                _repos.Add(new RepositoryViewModel(repo));
            }
        }

        public void Clear()
        {
            _selectedRepo = null;
            _repos.Clear();
        }

        public void OpenFirst()
        {
            if (_selectedRepo != null)
            {
                _selectedRepo.Open();
                return;
            }

            var repo = _repos.FirstOrDefault();
            if (repo == null)
            {
                return;
            }

            repo.Open();
        }

        private void KeyboardService_DownPressed(object sender, System.EventArgs e)
        {
            if (_selectedRepo == null)
            {
                _selectedRepo = _repos.FirstOrDefault();
                _selectedRepo.Select();
                return;
            }

            var index = _repos.IndexOf(_selectedRepo);
            if (index == -1 || (index + 1) >= _repos.Count)
            {
                return;
            }

            index++;
            _selectedRepo.IsSelected = false;
            _selectedRepo = Repos.ElementAt(index);
            _selectedRepo.Select();
        }

        private void KeyboardService_UpPressed(object sender, System.EventArgs e)
        {
            if (_selectedRepo == null)
            {
                return;
            }

            var index = _repos.IndexOf(_selectedRepo);
            if (index <= 0)
            {
                return;
            }

            index--;
            _selectedRepo.IsSelected = false;
            _selectedRepo = Repos.ElementAt(index);
            _selectedRepo.Select();
        }

        #endregion
    }
}
