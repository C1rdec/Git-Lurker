using Lurker.AppData;

namespace GitLurker.Core.Models
{
    public class GameSettingsFile : AppDataFileBase<GameSettings>
    {
        #region Fields

        private static readonly int MaxRecentCount = 5;

        #endregion

        #region Properties

        protected override string FileName => "Games.json";

        protected override string FolderName => "GitLurker";

        #endregion

        #region Methods

        public void AddRecent(string gameId)
        {
            var games = Entity.RecentGameIds;
            var index = games.IndexOf(gameId);
            if (index == -1)
            {
                if (games.Count >= MaxRecentCount)
                {
                    games.RemoveAt(MaxRecentCount - 1);
                }
            }
            else
            {
                games.RemoveAt(index);
            }

            Entity.RecentGameIds.Insert(0, gameId);
            Save();
        }

        public void RemoveRecent(string gameId)
        {
            var games = Entity.RecentGameIds;
            var index = games.IndexOf(gameId);
            if (index == -1)
            {
                return;
            }

            games.RemoveAt(index);
            Save();
        }

        public void SetSteamExePath(string path)
        {
            Entity.SteamExePath = path;
            Entity.SteamAsked = true;
            Save();
        }

        public void SetEpicExePath(string path)
        {
            Entity.EpicExePath = path;
            Entity.EpicAsked = true;
            Save();
        }

        #endregion
    }
}
