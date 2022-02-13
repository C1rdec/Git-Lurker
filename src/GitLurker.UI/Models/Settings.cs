using System.Collections.Generic;

namespace GitLurker.UI.Models
{
    public class Settings
    {
        #region Constructors

        public Settings()
        {
            Workspaces = new List<string>();
            LastFiveRepos = new List<string>();
        }

        #endregion

        public List<string> Workspaces { get; set; }

        public List<string> LastFiveRepos { get; set; }
    }
}
