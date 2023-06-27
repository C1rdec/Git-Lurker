﻿using System.Linq;
using Lurker.AppData;

namespace GitLurker.Core.Models
{
    public class SettingsFile : AppDataFileBase<Settings>
    {
        #region Fields

        private static readonly int MaxRecentCount = 5;

        #endregion

        #region Properties

        protected override string FileName => "Workspaces.json";

        protected override string FolderName => "GitLurker";

        #endregion

        #region Methods

        public void AddRecent(string folder)
        {
            var recentRepos = Entity.RecentRepos;
            var index = recentRepos.IndexOf(folder);
            if (index == -1)
            {
                if (recentRepos.Count >= MaxRecentCount)
                {
                    recentRepos.RemoveAt(MaxRecentCount - 1);
                }
            }
            else
            {
                recentRepos.RemoveAt(index);
            }

            Entity.RecentRepos.Insert(0, folder);
            Save();
        }

        public void RemoveRecent(string folder)
        {
            var recentRepos = Entity.RecentRepos;
            var index = recentRepos.IndexOf(folder);
            if (index == -1)
            {
                return;
            }

            recentRepos.RemoveAt(index);
            Save();
        }

        public void RemoveWorkspace(string folderPath)
        {
            var workspace = Entity.Workspaces.FirstOrDefault(w => w == folderPath);
            if (workspace == null)
            {
                return;
            }

            Entity.Workspaces.Remove(workspace);
            Save();
        }

        public bool HasNugetSource() => !string.IsNullOrEmpty(Entity.NugetSource);

        #endregion
    }
}