using System.Collections.Generic;

namespace GitLurker.Models
{
    public class Settings
    {
        #region Constructors

        public Settings()
        {
            Workspaces = new List<string>();
            RecentRepos = new List<string>();
        }

        #endregion

        public List<string> Workspaces { get; set; }

        public List<string> RecentRepos { get; set; }
    }
}
