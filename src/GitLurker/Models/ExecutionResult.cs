using System.Collections.Generic;

namespace GitLurker.Models
{
    public class ExecutionResult
    {
        #region Properties

        public IEnumerable<string> Output { get; set; }

        public int ExitCode { get; set; }

        #endregion
    }
}
