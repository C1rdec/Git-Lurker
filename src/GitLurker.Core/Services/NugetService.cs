namespace GitLurker.Core.Services;

using System.IO;
using System.Linq;
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

    #region Methods

    public Task<ExecutionResult> AddNugetAsync(NugetInformation nuget)
    {
        var settings = new SettingsFile();
        settings.Initialize();

        if (!settings.HasNugetSource())
        {
            return null;
        }

        return ExecuteCommandAsync($@"{GetNugetExe(settings.Entity.NugetSource)} add ""{nuget.FilePath}"" -source {settings.Entity.NugetSource}", true);
    }

    public Task<ExecutionResult> ListNugetAsync()
    {
        var settings = new SettingsFile();
        settings.Initialize();

        if (!settings.HasNugetSource())
        {
            return null;
        }

        return ExecuteCommandAsync($@"{GetNugetExe(settings.Entity.NugetSource)} list -source {settings.Entity.NugetSource} -prerelease");
    }

    public async Task<NugetInformation> GetNewNugetAsync()
    {
        NugetInformation newNuget = null;
        var executionResult = await ListNugetAsync();

        // To get out of the UI Thread
        await Task.Run(() =>
        {
            var filePaths = GetFiles($"*.nupkg", int.MaxValue).Where(n => n.FullName.Contains("\\bin\\"));
            if (filePaths.Any())
            {
                var nugetsInRepo = filePaths.Select(p => NugetInformation.Parse(p.FullName));

                var list = nugetsInRepo.ToList();
                list.Sort((n1, n2) => n2.CompareTo(n1));
                newNuget = list.FirstOrDefault();
            }
        });

        if (newNuget != null && executionResult != null)
        {
            // If the package is installedd
            if (executionResult.Output.Contains(newNuget.Name))
            {
                return null;
            }
        }

        return newNuget;
    }

    private string GetNugetExe(string source)
        => Directory.GetFiles(source, "nuget.exe").FirstOrDefault() ?? "nuget";

    #endregion
}
