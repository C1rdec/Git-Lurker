namespace GitLurker.UI.ViewModels
{
    using System.Collections.ObjectModel;
    using GitLurker.Models;

    public class WorkspaceViewModel
    {
        #region Fields

        private Workspace _workspace;
        private ObservableCollection<RepositoryViewModel> _repos;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceViewModel"/> class.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public WorkspaceViewModel(Workspace workspace)
        {
            this._workspace = workspace;
            this._repos = new ObservableCollection<RepositoryViewModel>();
            foreach(var repo in this._workspace.Repositories)
            {
                this._repos.Add(new RepositoryViewModel(repo));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the repos.
        /// </summary>
        /// <value>The repos.</value>
        public ObservableCollection<RepositoryViewModel> Repos => this._repos;

        #endregion
    }
}
