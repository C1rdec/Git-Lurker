namespace GitLurker.UI.ViewModels
{
    using GitLurker.Models;

    public class RepositoryViewModel : Caliburn.Micro.PropertyChangedBase
    {
        #region Fields

        private Repository _repo;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryViewModel"/> class.
        /// </summary>
        /// <param name="repo">The repo.</param>
        public RepositoryViewModel(Repository repo)
        {
            _repo = repo;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string RepoName => this._repo.Name;

        #endregion

        #region Methods

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public void Open()
        {
            _repo.Open();
        }

        #endregion
    }
}
