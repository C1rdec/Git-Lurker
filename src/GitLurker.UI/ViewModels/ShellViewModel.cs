namespace GitLurker.UI.ViewModels
{
    using System.Threading;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;

    public class ShellViewModel : PropertyChangedBase, IHandle<object>
    {
        #region Fields

        private string _searchTerm;
        private DebounceService _debounceService;
        private bool _isVisible;
        private IEventAggregator _eventAggregator;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel(IEventAggregator aggregator)
        {
            _isVisible = true;
            _eventAggregator = aggregator;
            _debounceService = new DebounceService();
            WorkspaceViewModel = new WorkspaceViewModel(new Workspace(@"D:\Github"));

            _eventAggregator.SubscribeOnPublishedThread(this);
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
                Search(_searchTerm);
                NotifyOfPropertyChange();
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        public void Search(string term)
        {
            _debounceService.Debounce(50, () => WorkspaceViewModel.Search(term));
        }

        public Task HandleAsync(object message, CancellationToken cancellationToken)
        {
            IsVisible = false;
            return Task.CompletedTask;
        }

        public void Close()
        {
            IsVisible = false;
        }

        #endregion
    }
}
