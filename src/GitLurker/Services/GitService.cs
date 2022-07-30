using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public bool IsBehind()
            => Execute(r => r.Head.TrackingDetails.BehindBy > 0);

        public string GetCurrentBranchName()
            => Execute(r => r.Head.FriendlyName);

        public IEnumerable<string> GetBranchNames()
        {
            var branchNames = new List<string>();
            var branches = Execute(r => r.Branches.Where(b => b.FriendlyName != "origin/HEAD"));
            
            foreach (var branch in branches.Where(b => !b.IsRemote))
            {
                branchNames.Add(branch.FriendlyName);
            }

            foreach (var remoteBrach in branches.Where(b => b.IsRemote))
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
            

        private T Execute<T>(System.Func<LibGit2Sharp.Repository, T> action)
        {
            var repo = new LibGit2Sharp.Repository(_gitFolderPath);
            return action(repo);
        }

        #endregion
    }
}
