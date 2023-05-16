using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitLurker.Core.Services
{
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
            var branches = Execute(r => r.Branches.Where(b => b.FriendlyName != "origin/HEAD").ToArray());

           foreach (var remoteBrach in branches.Where(b => b.IsRemote).OrderBy(b => b.FriendlyName))
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

        public IEnumerable<string> GetFilesChanged()
        {
            return Execute(repo => 
            {
                var filePaths = new List<string>();
                var status = repo.RetrieveStatus();

                foreach (var change in status.Modified)
                {
                    filePaths.Add(change.FilePath);
                }

                foreach (var change in status.Missing)
                {
                    if (change.State == FileStatus.DeletedFromWorkdir)
                    {
                        filePaths.Add(change.FilePath);
                    }
                }

                foreach (var change in status.Untracked)
                {
                    if (change.State == FileStatus.NewInWorkdir)
                    {
                        filePaths.Add(change.FilePath);
                    }
                }

                return filePaths;
            });
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
}
