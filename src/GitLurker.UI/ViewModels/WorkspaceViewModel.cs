namespace GitLurker.UI.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.Core.Services;
using GitLurker.UI.Services;

public class WorkspaceViewModel : PropertyChangedBase, IItemListViewModel
{
    #region Fields

    private static readonly Regex SshRegex = new(@"^(git@|ssh://)", RegexOptions.IgnoreCase);

    private KeyboardService _keyboardService;
    private RepositoryService _repositoryService;
    private ConsoleService _consoleService;
    private ObservableCollection<RepositoryViewModel> _repos;
    private RepositoryViewModel _selectedRepo;
    private string _lastSearchTerm;
    private bool _mouseOver;

    #endregion

    #region Constructors

    public WorkspaceViewModel(RepositoryService repositoryService, ConsoleService consoleService, KeyboardService keyboardService)
    {
        _repos = [];
        _repositoryService = repositoryService;
        _consoleService = consoleService;
        _keyboardService = keyboardService;

        _keyboardService.RightPressed += KeyboardService_RightPressed;
    }

    #endregion

    #region Properties

    public ObservableCollection<RepositoryViewModel> Repos => _repos;

    public bool HasSelectedRepo => SelectedRepo != null || !string.IsNullOrEmpty(_lastSearchTerm) || _mouseOver;

    public RepositoryViewModel SelectedRepo
    {
        get
        {
            return _selectedRepo;
        }

        private set
        {
            _selectedRepo = value;
            NotifyOfPropertyChange(() => HasSelectedRepo);
        }
    }

    #endregion

    #region Methods

    public async Task<bool> Open(bool skipModifier)
    {
        if (Uri.TryCreate(_lastSearchTerm, UriKind.Absolute, out var result) || SshRegex.IsMatch(_lastSearchTerm))
        {
            var repoUrl = _lastSearchTerm;
            _lastSearchTerm = string.Empty;
            await CloneAsync(repoUrl);

            return false;
        }

        ExecuteOnRepo((r) => r.Open(skipModifier));

        return true;
    }

    public async Task CloneAsync(string url)
    {
        var workspace = _repositoryService.Workspaces.FirstOrDefault();
        if (workspace == null)
        {
            return;
        }

        _consoleService.Listen(workspace);
        var newRepo = await workspace.CloneAsync(url);
        if (newRepo != null)
        {
            workspace.AddRepo(newRepo);
            Execute.OnUIThread(() => _repos.Insert(0, new RepositoryViewModel(newRepo)));
        }

        return;
    }

    public void MoveUp()
    {
        if (SelectedRepo == null)
        {
            return;
        }

        if (SelectedRepo.IsBranchManagerOpen)
        {
            SelectedRepo.SelectPreviousBranch();
            return;
        }

        var index = _repos.IndexOf(SelectedRepo);
        if (index <= 0)
        {
            return;
        }

        index--;
        SelectedRepo.UnSelect();
        SelectedRepo = Repos.ElementAt(index);
        SelectedRepo.Select();
    }

    public void MoveDown()
    {
        if (SelectedRepo == null)
        {
            SelectedRepo = _repos.FirstOrDefault();
            SelectedRepo?.Select();
            return;
        }

        if (SelectedRepo.IsBranchManagerOpen)
        {
            SelectedRepo.SelectNextBranch();
            return;
        }

        var index = _repos.IndexOf(_selectedRepo);
        if (index == -1 || (index + 1) >= _repos.Count)
        {
            return;
        }

        index++;
        SelectedRepo.UnSelect();
        SelectedRepo = Repos.ElementAt(index);
        SelectedRepo.Select();
    }

    public void OnMouseEnter()
    {
        _mouseOver = true;
        NotifyOfPropertyChange(() => HasSelectedRepo);
    }

    public void OnMouseLeave()
    {
        _mouseOver = false;
        NotifyOfPropertyChange(() => HasSelectedRepo);
    }

    public void Search(string term)
    {
        _lastSearchTerm = term;
        Clear();

        if (string.IsNullOrEmpty(term))
        {
            ShowRecent();
            return;
        }

        var repos = _repositoryService.Search(term);
        foreach (var repo in repos)
        {
            Repos.Add(new RepositoryViewModel(repo));
        }
    }

    public void Clear()
    {
        SelectedRepo = null;
        Execute.OnUIThread(() => _repos.Clear());
    }

    public bool Close()
    {
        if (SelectedRepo != null && SelectedRepo.IsBranchManagerOpen)
        {
            SelectedRepo.IsBranchManagerOpen = false;
            return false;
        }

        Clear();
        return true;
    }

    public void ShowRecent()
    {
        Clear();
        var file = new SettingsFile();
        file.Initialize();
        var invalidFolders = new List<string>();

        foreach (var folder in file.Entity.RecentRepos)
        {
            if (!Directory.Exists(folder))
            {
                invalidFolders.Add(folder);
                continue;
            }

            var repo = _repositoryService.GetRepository(folder);
            if (repo != null)
            {
                Execute.OnUIThread(() => Repos.Add(new RepositoryViewModel(repo)));
            }
        }

        foreach (var folder in invalidFolders)
        {
            file.RemoveRecent(folder);
        }
    }

    public async Task RefreshItems()
    {
        await Task.Run(() => _repositoryService.GetWorkspaces());
        Execute.OnUIThread(() => _repos.Clear());
    }

    public void NextTabPressed()
    {
        SelectedRepo ??= _repos.FirstOrDefault();

        if (SelectedRepo == null)
        {
            return;
        }

        SelectedRepo.Select();

        if (SelectedRepo.IsBranchManagerOpen)
        {
            SelectedRepo.OpenNewBranch();
        }
        else
        {
            SelectedRepo.ShowBranches(false);
        }
    }

    public void EnterLongPressed()
        => ExecuteOnRepo((r) => r.OpenPullRequest());

    private void ExecuteOnRepo(System.Action<RepositoryViewModel> action)
    {
        if (SelectedRepo != null)
        {
            action(SelectedRepo);
            return;
        }

        var firstRepo = _repos.FirstOrDefault();
        if (firstRepo != null)
        {
            action(firstRepo);
        }
    }

    private void KeyboardService_RightPressed(object sender, EventArgs e)
    {
        if (SelectedRepo == null)
        {
            return;
        }

        ExecuteOnRepo(r =>
        {
            r.OpenFileChanged();
        });
    }

    #endregion
}
