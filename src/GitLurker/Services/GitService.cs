﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitLurker.Services
{
    public class GitService
    {
        #region Fields

        private string _gitFolderPath;

        #endregion

        #region Constructors

        public GitService(string gitFolderPath)
        {
            _gitFolderPath = Path.Combine(gitFolderPath, ".git");
        }

        #endregion

        #region Methods

        public void Fetch()
            => Execute(r => Commands.Fetch(r, "origin", new string[0], null, null));

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

        public IEnumerable<string> GetFilesChanged()
        {
            return Execute(repo => 
            {
                var filePaths = new List<string>();
                foreach (var change in repo.Diff.Compare<TreeChanges>().Where(c => c.Status == ChangeKind.Modified))
                {
                    filePaths.Add(change.Path);
                }

                return filePaths;
            });
        }


        private T Execute<T>(System.Func<Repository, T> action)
        {
            using var repo = new Repository(_gitFolderPath);

            return action(repo);
        }

        private void Execute(System.Action<Repository> action)
        {
            using var repo = new Repository(_gitFolderPath);

            action(repo);
        }

        #endregion
    }
}
