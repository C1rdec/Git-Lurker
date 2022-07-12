using System;
using GitLurker.Services;

namespace GitLurker.UI.Services
{
    public class ConsoleService
    {
        #region Events

        public event EventHandler<ProcessService> ExecutionRequested;

        public event EventHandler ShowRequested;

        #endregion

        #region Methods

        public void Listen(ProcessService process) => ExecutionRequested?.Invoke(this, process);

        public void Show() => ShowRequested?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}
