namespace GitLurker.UI.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;

    public class WorkspaceViewModel: PropertyChangedBase
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

        public RepositoryViewModel SelectedRepo
        {
            get
            {
                return _selectedRepo;
            }

            private set
            {
                _selectedRepo = value;
                this.NotifyOfPropertyChange(() => HasSelectedRepo);
            }
        }

        public bool HasSelectedRepo => SelectedRepo != null;

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
            SelectedRepo = null;
            _repos.Clear();
        }

        public void Open()
        {
            if (SelectedRepo != null)
            {
                SelectedRepo.Open();
                return;
            }

            _repos.FirstOrDefault()?.Open();
        }

        private void KeyboardService_DownPressed(object sender, System.EventArgs e)
        {
            if (SelectedRepo == null)
            {
                SelectedRepo = _repos.FirstOrDefault();
                SelectedRepo.Select();
                return;
            }

            var index = _repos.IndexOf(_selectedRepo);
            if (index == -1 || (index + 1) >= _repos.Count)
            {
                return;
            }

            index++;
            SelectedRepo.IsSelected = false;
            SelectedRepo = Repos.ElementAt(index);
            SelectedRepo.Select();
        }

        private void KeyboardService_UpPressed(object sender, System.EventArgs e)
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
            SelectedRepo.IsSelected = false;
            SelectedRepo = Repos.ElementAt(index);
            SelectedRepo.Select();
        }

        #endregion
    }
}
