namespace GitLurker.UI.ViewModels
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;
    using GitLurker.UI.Models;
    using NHotkey.Wpf;
    using System.Windows.Input;
    using System.Windows;
    using GitLurker.UI.Helpers;

    public class ShellViewModel : Screen, IHandle<object>
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
        public ShellViewModel(IEventAggregator aggregator, SettingsFile settings)
        {
            _isVisible = true;
            _eventAggregator = aggregator;
            _debounceService = new DebounceService();
            var first = settings.Entity.Workspaces.FirstOrDefault();
            if (first != null)
            {
                WorkspaceViewModel = new WorkspaceViewModel(new Workspace(settings.Entity.Workspaces.FirstOrDefault()));
            }

            _eventAggregator.SubscribeOnPublishedThread(this);
            HotkeyManager.Current.AddOrReplace("Increment", Key.G, ModifierKeys.Control , (s, e) => Show());
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

        protected Window View { get; private set; }

        #endregion

        #region Methods

        public void Search(string term)
        {
            if (WorkspaceViewModel == null)
            {
                return;
            }

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
            SearchTerm = string.Empty;
        }

        public void Show()
        {
            IsVisible = true;
            DockingHelper.SetForeground(View);
        }

        protected override void OnViewLoaded(object view)
        {
            this.View = view as Window;
        }

        #endregion
    }
}
