﻿using System.Collections.Generic;

namespace GitLurker.Models
{
    public class GameSettings
    {
        #region Constructors

        public GameSettings()
        {
            RecentGameIds = new List<string>();
            Scheme = Scheme.Crimson;
        }

        #endregion

        #region Properties

        public List<string> RecentGameIds { get; set; }

        public Scheme Scheme { get; set; }

        public string SteamExePath { get; set; }

        public string EpicExePath { get; set; }

        #endregion
    }
}