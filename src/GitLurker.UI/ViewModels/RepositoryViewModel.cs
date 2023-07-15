namespace GitLurker.UI.ViewModels
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using Caliburn.Micro;
    using GitLurker.Core.Models;
    using GitLurker.UI.Messages;
    using GitLurker.UI.Services;
    using MahApps.Metro.IconPacks;

    public class RepositoryViewModel : ItemViewModelBase
    {
        #region Fields

        private static readonly CloseMessage CloseMessage = new();
        private CancellationTokenSource _nugetTokenSource;
        private CancellationTokenSource _secretTokenSource;
        private CancellationTokenSource _pullRequestTokenSource;
        private bool _isBranchManagerOpen;
        private bool _isFileChangedOpen;
        private PopupService _popupService;
        private ConsoleService _consoleService;
        private Repository _repo;
        private IEventAggregator _eventAggregator;
        private string _branchName;
        private bool _busy;
        private bool _showParentFolder;
        private ActionBarViewModel _actionBar;
        private SettingsFile _settingsFile;
        private bool _skipBranchSelection;
        private bool _operationInProgress;
        private bool _cancelOperationVisible;
        private bool _skipOpen;
        private bool _hasStashes;
        private bool _stashLoading;
        private bool _isRunning;

        #endregion

        #region Constructors

        public RepositoryViewModel(Repository repo)
        {
            _repo = repo;
            _isRunning = repo.IsRunning;
            _settingsFile = IoC.Get<SettingsFile>();
            _popupService = IoC.Get<PopupService>();
            _consoleService = IoC.Get<ConsoleService>();
            _actionBar = new ActionBarViewModel(repo);
            _showParentFolder = repo.Duplicate;
            _eventAggregator = IoC.Get<IEventAggregator>();

            FileChanges = new ObservableCollection<string>();
            BranchManager = new BranchManagerViewModel(repo, OnSelectionChanged, OnBranchManagerClose, Rebase);
            GetStatus();
        }

        #endregion

        #region Properties

        public ObservableCollection<string> FileChanges { get; set; }

        public BranchManagerViewModel BranchManager { get; private set; }

        public ActionBarViewModel ActionBar => _actionBar;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFileChangedOpen
        {
            get => _isFileChangedOpen;
            set
            {
                _isFileChangedOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public bool HasIcon => _repo.HasIcon;

        public bool IsVsCode => !_repo.HasSln;

        public BitmapFrame IconSource
        {
            get
            {
                if (!File.Exists(_repo.IconPath))
                {
                    return null;
                }

                using var stream = new FileStream(_repo.IconPath, FileMode.Open, FileAccess.Read);

                return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }

        public string RepoName => _repo.Name;

        public bool OperationInProgress
        {
            get => _operationInProgress;
            set
            {
                _operationInProgress = value;
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
                NotifyOfPropertyChange(() => CancelOperationVisisble);
            }
        }

        public bool BranchNameVisible => !string.IsNullOrEmpty(_branchName) || OperationInProgress;

        public bool CancelOperationVisisble
        {
            get => _cancelOperationVisible;
            set
            {
                _cancelOperationVisible = value;
                NotifyOfPropertyChange();
            }
        }

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
            get => _isBranchManagerOpen;
            set
            {
                _popupService.IsOpen = value;
                _isBranchManagerOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public bool HasStashes
        {
            get => _hasStashes;
            set
            {
                _hasStashes = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsStashLoading
        {
            get => _stashLoading;
            set
            {
                _stashLoading = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsNotStashLoading);
            }
        }

        public bool IsNotStashLoading => !IsStashLoading;

        public string ParentFolderName => GetParentFolder();

        public bool HasFilesChanged => FileChanges.Any();

        public int FileChangeCount => FileChanges.Count;

        #endregion

        #region Methods

        public async void CheckoutLastBranch()
        {
            _skipOpen = true;
            await _repo.CheckoutLastBranchAsync();
            BranchName = _repo.GetCurrentBranchName();
        }

        public async void StartDefaultProject()
        {
            _skipOpen = true;

            IsRunning = true;
            await _repo.StartDefaultProject();
            IsRunning = false;
        }

        public void OnPopupClosed() => _popupService.SetClosed();

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

            if (OperationInProgress)
            {
                CancelOperation();
                return;
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

                await _eventAggregator.PublishOnCurrentThreadAsync(CloseMessage);
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
            _pullRequestTokenSource?.Cancel();

            if (_popupService.JustClosed || _skipOpen)
            {
                _skipOpen = false;

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

            _eventAggregator.PublishOnCurrentThreadAsync(CloseMessage);
            _repo.Open(skipModifier);
        }

        public void OpenPullRequest()
        {
            _eventAggregator.PublishOnCurrentThreadAsync(CloseMessage);
            _repo.OpenPullRequest();
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
            BranchName = _operationInProgress ? "Conflict(s)" : _repo.GetCurrentBranchName();
            CancelOperationVisisble = _operationInProgress;

            await HandleUserSecret();
            await HandleNuget();
        }

        public void OnMouseLeave()
        {
            if (!IsSelected)
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
            CancelOperationVisisble = false;
        }

        public void Select()
        {
            IsSelected = true;
            BranchName = _repo.GetCurrentBranchName();
        }

        public void UnSelect()
        {
            IsSelected = false;
            IsBranchManagerOpen = false;
            BranchName = string.Empty;
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

        public async void ContinueOperation()
        {
            _skipOpen = true;
            _consoleService.Listen(_repo);

            await _repo.ContinueOperationAsync();
            await GetStatus();

            if (OperationInProgress)
            {
                return;
            }

            CancelOperationVisisble = false;
            BranchName = _repo.GetCurrentBranchName();
        }

        public void OpenFileChanged()
        {
            IsFileChangedOpen = true;
        }

        public async void GitStash()
        {
            IsStashLoading = true;
            await Task.Run(() => _repo.Stash());
            await GetStatus();

            IsStashLoading = false;
        }

        public void GitStashPop()
        {
            _repo.Pop();
            GetStatus();
        }

        private async void CancelOperation()
        {
            _skipOpen = true;
            _consoleService.Listen(_repo);

            await _repo.CancelOperationAsync();
            await GetStatus();

            if (OperationInProgress)
            {
                return;
            }

            CancelOperationVisisble = false;
            BranchName = _repo.GetCurrentBranchName();
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

        private Task GetStatus()
            => Task.Run(() =>
            {
                OperationInProgress = _repo.HasOperationInProgress();
                var stashes = _repo.GetStashes();
                HasStashes = stashes != null && stashes.Any();

                var changes = _repo.GetFilesChanged();
                if (changes == null)
                {
                    return;
                }

                Execute.OnUIThread(() =>
                {
                    FileChanges.Clear();

                    foreach (var change in changes)
                    {
                        Execute.OnUIThread(() => FileChanges.Add(change));
                    }

                    NotifyOfPropertyChange(() => FileChangeCount);
                    NotifyOfPropertyChange(() => HasFilesChanged);
                    NotifyOfPropertyChange(() => BranchNameVisible);
                });
            });

        private async void Rebase(string branchName)
        {
            IsBranchManagerOpen = false;
            var currentBranch = _repo.GetCurrentBranchName();
            if (currentBranch == branchName)
            {
                return;
            }

            _consoleService.Listen(_repo);
            if (_settingsFile.Entity.RebaseOperation == LibGit2Sharp.CurrentOperation.Rebase)
            {
                await _repo.RebaseAsync($"origin/{branchName}");
            }
            else
            {
                await _repo.MergeAsync($"origin/{branchName}");
            }

            await GetStatus();
        }

        #endregion
    }
}
