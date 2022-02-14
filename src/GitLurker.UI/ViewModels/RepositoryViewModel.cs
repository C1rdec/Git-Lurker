namespace GitLurker.UI.ViewModels
{
    using Caliburn.Micro;
    using GitLurker.Models;

    public class RepositoryViewModel : PropertyChangedBase
    {
        #region Fields

        private static readonly object CloseObject = new();
        private Repository _repo;
        private bool _isSelected;

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

        public string RepoName => this._repo.Name;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        public void Open()
        {
            IoC.Get<IEventAggregator>().PublishOnCurrentThreadAsync(CloseObject);
            _repo.Open();
        }

        #endregion
    }
}
