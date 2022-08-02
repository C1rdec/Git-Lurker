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


        private T Execute<T>(System.Func<LibGit2Sharp.Repository, T> action)
        {
            using var repo = new LibGit2Sharp.Repository(_gitFolderPath);
            return action(repo);
        }

        #endregion
    }
}
