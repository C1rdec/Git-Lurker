namespace GitLurker.Core.Services;

using System.Threading.Tasks;
using GitLurker.Core.Models;

public class NugetService : ProcessService
{
    #region Constructors

    public NugetService(string folder)
        : base(folder)
    {
    }

    #endregion

    #region Properties

    public string NugetCommandBase => "nuget";

    public string NugetAddCommand => $"{NugetCommandBase} add";

    public string NugetListCommand => $"{NugetCommandBase} list";

    #endregion

    #region Methods

    public async Task<ExecutionResult> AddNugetAsync(NugetInformation nuget)
    {
        var settings = new SettingsFile();
        settings.Initialize();

        if (!settings.HasNugetSource())
        {
            return null;
        }


        var nugetSource = $"-source {settings.Entity.NugetSource}";
        var command = $@"{NugetAddCommand} ""{nuget.FilePath}"" {nugetSource}";

        return await ExecuteCommandAsync(command, true);
    }

    #endregion
}
