using AppDataFileManager;

namespace GitLurker.Models
{
    public class SettingsFile : AppDataFileBase<Settings>
    {
        private static readonly int MaxRecentCount = 5;

        protected override string FileName => "Workspaces.json";

        protected override string FolderName => "GitLurker";

        public void AddToRecent(string folder)
        {
            var recentRepos = Entity.RecentRepos;
            var index = recentRepos.IndexOf(folder);
            if (recentRepos.IndexOf(folder) == -1)
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
    }
}
