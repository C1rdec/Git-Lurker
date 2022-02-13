namespace GitLurker.UI.ViewModels
{
    using GitLurker.Models;
    using GitLurker.Services;

    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase
    {
        #region Fields

        private string _searchTerm;
        private DebounceService _debounceService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel()
        {
            this.WorkspaceViewModel = new WorkspaceViewModel(new Workspace(@"D:\Github"));
            _debounceService = new DebounceService();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title => ".NET 5!!!";

        /// <summary>
        /// Gets the workspace view model.
        /// </summary>
        /// <value>The workspace view model.</value>
        public WorkspaceViewModel WorkspaceViewModel { get; private set; }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                this.Search(_searchTerm);
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        public void Search(string term)
        {
            _debounceService.Debounce(200, () => WorkspaceViewModel.Search(term));
        }

        #endregion
    }
}
