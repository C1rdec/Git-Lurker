namespace GitLurker.UI.ViewModels
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.UI.Messages;
    using MahApps.Metro.IconPacks;

    public class RepositoryViewModel : Screen
    {
        #region Fields

        private static readonly CloseMessage CloseMessage = new();
        private CancellationTokenSource _tokenSource;
        private Repository _repo;
        private bool _isSelected;
        private IEventAggregator _aggregator;
        private string _branchName;
        private bool _busy;
        private bool _showParentFolder;
        private ActionBarViewModel _actionBar;
        private SettingsFile _settingsFile;

        #endregion

        #region Constructors

        public RepositoryViewModel(Repository repo)
        {
            _repo = repo;
            _settingsFile = IoC.Get<SettingsFile>();
            _actionBar = new ActionBarViewModel(repo);
            _showParentFolder = repo.Duplicate;
            _aggregator = IoC.Get<IEventAggregator>();
            _repo.NewProcessMessage += Repo_NewProcessMessage;
        }

        #endregion

        #region Properties

        public ActionBarViewModel ActionBar => _actionBar;

        public bool HasIcon => _repo.HasIcon;

        public string IconSource => _repo.IconPath;

        public string RepoName => _repo.Name;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        public string BranchName
        {
            get => _branchName;
            set
            {
                _branchName = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => BranchNameVisible);
            }
        }

        public bool BranchNameVisible => !string.IsNullOrEmpty(_branchName);

        public bool Busy
        {
            get => _busy;
            set
            {
                _busy = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => NotBusy);
            }
        }

        public bool NotBusy => !Busy;

        public bool ShowParentFolder
        {
            get => _showParentFolder;
            set
            {
                _showParentFolder = value;
                NotifyOfPropertyChange();
            }
        }

        public string ParentFolderName => GetParentFolder();

        #endregion

        #region Methods

        public async void Delay()
        {
            if (_tokenSource != null)
            {
                return;
            }

            try
            {
                _tokenSource = new CancellationTokenSource();
                await Task.Delay(600);
                if (_tokenSource.IsCancellationRequested)
                {
                    return;
                }

                await _aggregator.PublishOnCurrentThreadAsync(CloseMessage);
                _repo.OpenPullRequest();
            }
            finally
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }
        }

        public void Open() => Open(skipModifier: false);

        public void Open(bool skipModifier)
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }

            _aggregator.PublishOnCurrentThreadAsync(CloseMessage);
            _repo.Open(skipModifier);
        }

        public async void Pull() 
        {
            if (Busy)
            {
                return;
            }

            Busy = true;
            await _repo.PullAsync();
            Busy = false;
        }

        public async void OnMouseEnter()
        {
            BranchName = _repo.GetCurrentBranchName();

            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }

            if (_settingsFile.HasNugetSource())
            {
                _tokenSource = new CancellationTokenSource();
                var token = _tokenSource.Token;
                var nuget = await _repo.GetNewNugetAsync(_settingsFile.Entity.NugetSource);
                if (nuget != null)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    var action = new ActionViewModel(() => _repo.AddNugetAsync(nuget), new PackIconSimpleIcons() { Kind = PackIconSimpleIconsKind.NuGet }, false);
                    _actionBar.AddAction(action);
                }
            }
        }

        public void OnMouseLeave()
        {
            BranchName = string.Empty;
            _tokenSource?.Cancel();
            _actionBar.RemoveActions();
        }

        public void Select()
        {
            IsSelected = true;
        }

        private string GetParentFolder()
        {
            var segments = _repo.Folder.Split('\\');

            return $"({segments[^1]})";
        }

        private void Repo_NewProcessMessage(object sender, string e)
        {
            _aggregator.PublishOnUIThreadAsync(e);
        }

        #endregion
    }
}
