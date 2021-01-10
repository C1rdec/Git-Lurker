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
            this._repo = repo;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this._repo.Name;

        #endregion
    }
}
