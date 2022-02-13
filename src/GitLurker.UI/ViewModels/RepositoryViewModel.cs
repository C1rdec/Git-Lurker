namespace GitLurker.UI.ViewModels
{
    using Caliburn.Micro;
    using GitLurker.Models;

    public class RepositoryViewModel : PropertyChangedBase
    {
        #region Fields

        private static object CloseObject = new();
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
            IoC.Get<IEventAggregator>().PublishOnCurrentThreadAsync(CloseObject);
            _repo.Open();
        }

        #endregion
    }
}
