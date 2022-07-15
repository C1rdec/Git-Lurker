using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitLurker.Extensions;

namespace GitLurker.Services
{
    public class GitConfigurationService
    {
        #region Fields

        private string _gitFolderPath;

        #endregion

        #region Constructors

        public GitConfigurationService(string gitFolderPath)
        {
            _gitFolderPath = Path.Combine(gitFolderPath, ".git");
        }

        #endregion

        #region Methods

        public string GetCurrentBranchName()
        {
            var headFilePath = Path.Combine(_gitFolderPath, "HEAD");
            if (!File.Exists(headFilePath))
            {
                return string.Empty;
            }

            return File.ReadAllText(headFilePath).GetLineAfter("ref: refs/heads/");
        }

        public IEnumerable<string> GetBranchNames()
        {
            var headFilePath = Path.Combine(_gitFolderPath, "refs", "remotes", "origin");
            if (!Directory.Exists(headFilePath))
            {
                return new List<string>() { GetCurrentBranchName() };
            }

            var files = Directory.GetFiles(headFilePath, "*.*", SearchOption.AllDirectories);
            var currentBranch = GetCurrentBranchName();
            var branches = files.Where(f => !f.EndsWith("\\HEAD"))
                                .Select(f => f.Substring(headFilePath.Length + 1).Replace(@"\", @"/")).ToList();

            var index = branches.Remove(currentBranch);
            branches.Insert(0, currentBranch);

            return branches;
        }

        #endregion
    }
}
