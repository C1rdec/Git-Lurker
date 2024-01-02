namespace GitLurker.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

public class GitService
{
    #region Fields

    private static readonly List<CurrentOperation> RebaseOperations = new()
    {
        CurrentOperation.Rebase,
        CurrentOperation.RebaseMerge,
        CurrentOperation.RebaseInteractive,
    };

    private static readonly List<CurrentOperation> MergeOperations = new()
    {
        CurrentOperation.Merge,
    };

    private string _gitFolderPath;

    #endregion

    #region Constructors

    public GitService(string gitFolderPath)
    {
        _gitFolderPath = Path.Combine(gitFolderPath, ".git");
    }

    #endregion

    #region Methods

    public CurrentOperation GetCurrentOperation()
    {
        return Execute(r =>
        {
            var currentOperation = r.Info.CurrentOperation;

            if (RebaseOperations.Contains(currentOperation))
            {
                return CurrentOperation.Rebase;
            }

            if (MergeOperations.Contains(currentOperation))
            {
                return CurrentOperation.Merge;
            }

            return currentOperation;
        });
    }

    public void Fetch()
        => Execute(r => Commands.Fetch(r, "origin", Array.Empty<string>(), null, null));

    public bool IsBehind()
        => Execute(r => r.Head.TrackingDetails.BehindBy > 0);

    public string GetCurrentBranchName()
        => Execute(r => r.Head.FriendlyName);

    public IEnumerable<string> GetBranchNames()
    {
        var branchNames = new List<string>()
        {
            GetCurrentBranchName().Replace("origin/", string.Empty)
        };

        foreach (var remoteBrach in GetRemoteBranches())
        {
            var branchName = remoteBrach.FriendlyName.Replace("origin/", string.Empty);
            if (branchNames.Contains(branchName))
            {
                continue;
            }

            branchNames.Add(branchName);
        }

        return branchNames;
    }

    public bool HasOperationInProgress() => Execute(r => r.Info.CurrentOperation != CurrentOperation.None);

    public IEnumerable<Stash> GetStashes()
        => Execute(r => r.Stashes.ToArray());

    public void Stash()
        => Execute(r => r.Stashes.Add(new Signature("GitLurker", "git_lurker@gmail.com", DateTime.Now)));

    public void Pop()
        => Execute(r => r.Stashes.Pop(r.Stashes.Count() - 1));

    public IEnumerable<Models.FileChange> GetFilesChanged()
    {
        return Execute(repo =>
        {
            var filePaths = new List<Models.FileChange>();
            var status = repo.RetrieveStatus();

            foreach (var change in status.Modified)
            {
                filePaths.Add(new Models.FileChange
                {
                    FilePath = change.FilePath,
                    Status = Models.ChangeStatus.Modified,
                });
            }

            foreach (var change in status.Missing)
            {
                if (change.State == FileStatus.DeletedFromWorkdir)
                {
                    filePaths.Add(new Models.FileChange
                    {
                        FilePath = change.FilePath,
                        Status = Models.ChangeStatus.Deleted,
                    });
                }
            }

            foreach (var change in status.Untracked)
            {
                if (change.State == FileStatus.NewInWorkdir)
                {
                    filePaths.Add(new Models.FileChange
                    {
                        FilePath = change.FilePath,
                        Status = Models.ChangeStatus.Added,
                    });
                }
            }

            return filePaths;
        });
    }

    public List<Branch> GetRemoteBranches()
    {
        var branches = Execute(r => r.Branches.Where(b => b.FriendlyName != "origin/HEAD" && b.IsRemote).ToArray());

        return branches.OrderBy(b => b.FriendlyName).ToList();
    }

    private T Execute<T>(Func<Repository, T> action, T defaultValue)
    {
        if (!Directory.Exists(_gitFolderPath))
        {
            return default;
        }

        try
        {
            using var repo = new Repository(_gitFolderPath);
            return action(repo);
        }
        catch
        {
            return defaultValue;
        }
    }

    private T Execute<T>(Func<Repository, T> action)
        => Execute(action, default);

    private void Execute(System.Action<Repository> action)
    {
        if (!Directory.Exists(_gitFolderPath))
        {
            return;
        }

        using var repo = new Repository(_gitFolderPath);

        action(repo);
    }

    #endregion
}
