using System.Collections.Generic;

namespace GitLurker.Models
{
    public class CustomAction
    {
        #region Constructors

        public CustomAction()
        {
            Repositories = new List<string>();
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public string Icon { get; set; }

        public string Command { get; set; }

        public List<string> Repositories { get; set; }

        public bool OpenConsole { get; set; }

        public bool ApplyToAll { get; set; }

        #endregion

        #region Methods

        public bool AddRepository(Repository repo)
        {
            if (Repositories.Contains(repo.Folder))
            {
                return false;
            }

            Repositories.Add(repo.Folder);
            return true;
        }

        public bool RemoveRepository(Repository repo)
        {
            return Repositories.Remove(repo.Folder);
        }

        #endregion
    }
}
