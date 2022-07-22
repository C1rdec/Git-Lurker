﻿using System.Collections.Generic;

namespace GitLurker.Models
{
    public class Settings
    {
        #region Constructors

        public Settings()
        {
            Workspaces = new List<string>();
            RecentRepos = new List<string>();
            HotKey = new Hotkey()
            {
                Modifier = Winook.Modifiers.Control,
                KeyCode = Winook.KeyCode.G,
            };
            DevToysHotKey = new Hotkey()
            {
                Modifier = Winook.Modifiers.None,
                KeyCode = Winook.KeyCode.None,
            };

            StartWithWindows = true;
            AddToStartMenu = true;
        }

        #endregion

        #region Properties

        public List<string> Workspaces { get; set; }

        public List<string> RecentRepos { get; set; }

        public Hotkey HotKey { get; set; }

        public Hotkey DevToysHotKey { get; set; }

        public bool ConsoleOuput { get; set; }

        public bool StartWithWindows { get; set; }

        public bool AddToStartMenu { get; set; }

        public string NugetSource { get; set; }

        public Scheme Scheme { get; set; }

        #endregion
    }
}
