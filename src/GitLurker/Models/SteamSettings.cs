using System.Collections.Generic;

namespace GitLurker.Models
{
    public class SteamSettings
    {
        #region Constructors

        public SteamSettings()
        {
            RecentGameIds = new List<string>();
            Scheme = Scheme.Crimson;
        }

        #endregion

        #region Properties

        public List<string> RecentGameIds { get; set; }

        public Scheme Scheme { get; set; }

        #endregion
    }
}
