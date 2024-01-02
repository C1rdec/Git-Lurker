namespace GitLurker.Core.Models;

using System.Collections.Generic;

public class ExecutionResult
{
    #region Properties

    public IEnumerable<string> Output { get; set; }

    public int ExitCode { get; set; }

    #endregion
}
