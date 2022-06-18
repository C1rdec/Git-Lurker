using GitLurker.Models;

namespace GitLurker.UI.Messages
{
    public class ConsoleMessage
    {
        #region Properties

        public bool OpenConsole { get; set; }

        public Repository Repository { get; set; }

        public string ActionName { get; set; }

        public bool Invalid => Repository == null;

        #endregion
    }
}
