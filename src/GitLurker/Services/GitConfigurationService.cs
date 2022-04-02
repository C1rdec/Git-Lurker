using System.IO;
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

        #endregion
    }
}
