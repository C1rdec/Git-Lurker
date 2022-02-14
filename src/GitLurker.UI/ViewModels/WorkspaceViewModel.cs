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
            _repos.Clear();

            var result = _workspace.Repositories.Where(r => r.Name.ToUpper().Contains(term.ToUpper())).ToList();
            foreach (var repo in result)
            {
                _repos.Add(new RepositoryViewModel(repo));
            }
        }

        public void OpenFirst()
        {
            var repo = _repos.FirstOrDefault();
            if (repo == null)
            {
                return;
            }

            repo.Open();
        }

        #endregion
    }
}
