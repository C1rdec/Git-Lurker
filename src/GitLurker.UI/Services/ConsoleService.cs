using System;
using GitLurker.Models;

namespace GitLurker.UI.Services
{
    public class ConsoleService
    {
        #region Events

        public event EventHandler<Repository> ExecutionRequested;

        public event EventHandler ShowRequested;

        #endregion

        #region Methods

        public void Listen(Repository repo) => ExecutionRequested?.Invoke(this, repo);

        public void Show() => ShowRequested?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}
