namespace GitLurker.Core.Services;

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
        => AddNugetAsync(nuget, false);

    public Task<ExecutionResult> AddNugetAsync(NugetInformation nuget, bool online)
    {
        var settings = new SettingsFile();
        settings.Initialize();

        if (!settings.HasNugetSource())
        {
            return null;
        }

        var source = online ? settings.Entity.RemoteNugetSource : settings.Entity.LocalNugetSource;
        var apiKey = online && !string.IsNullOrEmpty(settings.Entity.NugetApiKey) ? $"-k {settings.Entity.NugetApiKey}" : "";

        return ExecuteCommandAsync($@"dotnet nuget push ""{nuget.FilePath}"" {apiKey} -s {source} --skip-duplicate", true);
    }

    public async Task<NugetInformation> GetLatestNugetAsync()
    {
        NugetInformation newNuget = null;

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

        return newNuget;
    }

    #endregion
}
