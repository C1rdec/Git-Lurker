namespace GitLurker.UI.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.UI.Messages;
    using GitLurker.UI.Services;
    using MahApps.Metro.IconPacks;

    public class RepositoryViewModel : Screen
    {
        #region Fields

        private static readonly CloseMessage CloseMessage = new CloseMessage();
        private CancellationTokenSource _nugetTokenSource;
        private CancellationTokenSource _secretTokenSource;
        private CancellationTokenSource _pullRequestTokenSource;
        private PopupService _popupService;
        private Repository _repo;
        private bool _isSelected;
        private IEventAggregator _aggregator;
        private string _branchName;
        private bool _busy;
        private bool _showParentFolder;
        private ActionBarViewModel _actionBar;
        private SettingsFile _settingsFile;
        private bool _skipBranchSelection;

        #endregion

        #region Constructors

        public RepositoryViewModel(Repository repo)
        {
            _repo = repo;
            _settingsFile = IoC.Get<SettingsFile>();
            _popupService = IoC.Get<PopupService>();
            _actionBar = new ActionBarViewModel(repo);
            _showParentFolder = repo.Duplicate;
            _aggregator = IoC.Get<IEventAggregator>();

            FileChanges = new ObservableCollection<string>();
            BranchManager = new BranchManagerViewModel(repo, OnSelectionChanged, OnBranchManagerClose);
            GetChanges();
        }

        #endregion

        #region Properties

        public ObservableCollection<string> FileChanges { get; set; }

        public BranchManagerViewModel BranchManager { get; private set; }

        public ActionBarViewModel ActionBar => _actionBar;

        public bool HasIcon => _repo.HasIcon;

        public bool IsVsCode => !_repo.HasSln;

        public string IconSource => _repo.IconPath;

        public string RepoName => _repo.Name;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (value)
                {
                    BranchName = _repo.GetCurrentBranchName();
                }
                else
                {
                    BranchName = string.Empty;
                }

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

        public bool IsBranchManagerOpen
        {
            get => _popupService.IsOpen;
            set
            {   
                _popupService.IsOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public string ParentFolderName => GetParentFolder();

        public bool HasFilesChanged => FileChanges.Any();

        public int FileChangeCount => FileChanges.Count();

        #endregion

        #region Methods

        public void OnBranchManagerClosed() => _popupService.SetClosed();

        public void ShowBranches()
            => ShowBranches(true);

        public void ShowBranches(bool skipBranchSelection)
        {
            if (IsBranchManagerOpen || _popupService.JustClosed)
            {
                IsBranchManagerOpen = false;
                return;
            }

            if (skipBranchSelection)
            {
                _skipBranchSelection = true;
            }

            IsBranchManagerOpen = true;
            BranchManager.ShowBranches();
        }

        public void OnSelectionChanged(string branch)
        {
            BranchName = branch;
            IsBranchManagerOpen = false;
        }

        public void OnBranchManagerClose()
        {
            IsBranchManagerOpen = false;
        }

        public async void Delay()
        {
            if (_pullRequestTokenSource != null)
            {
                return;
            }

            try
            {
                _pullRequestTokenSource = new CancellationTokenSource();
                await Task.Delay(600);
                if (_pullRequestTokenSource.IsCancellationRequested)
                {
                    return;
                }

                await _aggregator.PublishOnCurrentThreadAsync(CloseMessage);
                _repo.OpenPullRequest();
            }
            finally
            {
                _pullRequestTokenSource.Dispose();
                _pullRequestTokenSource = null;
            }
        }

        public void Open() => Open(skipModifier: false);

        public void Open(bool skipModifier)
        {
            if (_pullRequestTokenSource != null)
            {
                _pullRequestTokenSource.Cancel();
            }

            if (_popupService.JustClosed)
            {
                return;
            }

            if (IsBranchManagerOpen)
            {
                if (!_skipBranchSelection)
                {
                    BranchManager.Execute();
                }

                _skipBranchSelection = false;
                return;
            }

            _aggregator.PublishOnCurrentThreadAsync(CloseMessage);
            _repo.Open(skipModifier);
        }

        public void OpenPullRequest() => _repo.OpenPullRequest();

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

            await HandleUserSecret();
            await HandleNuget();
        }

        public void OnMouseLeave()
        {
            if (!_isSelected)
            {
                BranchName = string.Empty;
            }

            if (_nugetTokenSource != null && !_nugetTokenSource.IsCancellationRequested)
            {
                _nugetTokenSource?.Cancel();
            }

            if (_secretTokenSource != null && !_secretTokenSource.IsCancellationRequested)
            {
                _secretTokenSource?.Cancel();
            }

            if (_pullRequestTokenSource != null && !_pullRequestTokenSource.IsCancellationRequested)
            {
                _pullRequestTokenSource?.Cancel();
            }

            _actionBar.RemoveActions();
        }

        public void Select()
        {
            IsSelected = true;
        }

        public void UnSelect()
        {
            IsSelected = false;
            IsBranchManagerOpen = false;
        }

        public void SelectNextBranch()
        {
            if (IsBranchManagerOpen)
            {
                BranchManager.SelectNextBranch();
            }
        }

        public void OpenNewBranch()
        {
            BranchManager.ShowCreateBranch();
        }

        public void SelectPreviousBranch()
        {
            if (IsBranchManagerOpen)
            {
                BranchManager.SelectPreviousBranch();
            }
        }

        public void ToggleBranches()
        {
            Select();
            if (IsBranchManagerOpen)
            {
                IsBranchManagerOpen = false;
            }
            else
            {
                ShowBranches(false);
            }
        }

        private string GetParentFolder()
        {
            var segments = _repo.Folder.Split('\\');

            return $"({segments[^1]})";
        }

        private async Task HandleNuget()
        {
            if (_nugetTokenSource != null && !_nugetTokenSource.IsCancellationRequested)
            {
                _nugetTokenSource.Cancel();
                _nugetTokenSource.Dispose();
            }

            if (_settingsFile.HasNugetSource() && _repo.Exist)
            {
                _nugetTokenSource = new CancellationTokenSource();
                var token = _nugetTokenSource.Token;
                var nuget = await _repo.GetNewNugetAsync(_settingsFile.Entity.NugetSource);
                if (nuget != null)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    var icon = new PackIconSimpleIcons() { Kind = PackIconSimpleIconsKind.NuGet };
                    _actionBar.AddAction(() => _repo.AddNugetAsync(nuget), icon, openConsole: false, permanent: false);
                }
            }
        }

        private Task HandleUserSecret()
        {
            if (_secretTokenSource != null && !_secretTokenSource.IsCancellationRequested)
            {
                _secretTokenSource.Cancel();
                _secretTokenSource.Dispose();
            }

            return Task.Run(() => 
            {
                _secretTokenSource = new CancellationTokenSource();
                var token = _secretTokenSource.Token;

                var secretId = _repo.GetUserSecretId();
                if (!string.IsNullOrEmpty(secretId))
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    Execute.OnUIThread(() =>
                    {
                        var icon = new PackIconFontisto() { Kind = PackIconFontistoKind.UserSecret };
                        _actionBar.AddAction(() => 
                        {
                            _repo.OpenUserSecret(secretId);
                            return Task.FromResult(new ExecutionResult());
                        }, icon, openConsole: false, permanent: false);
                    });
                }
            });
        }

        private Task GetChanges()
            => Task.Run(() =>
            {
                var changes = _repo.GetFilesChanged();
                if (changes == null)
                {
                    return;
                }

                Execute.OnUIThread(() => 
                {
                    foreach (var change in changes)
                    {
                        Execute.OnUIThread(() => FileChanges.Add(change));
                    }

                    NotifyOfPropertyChange(() => FileChangeCount);
                    NotifyOfPropertyChange(() => HasFilesChanged);
                });
            });

        #endregion
    }
}
