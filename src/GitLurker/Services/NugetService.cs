using System.Threading.Tasks;
using GitLurker.Models;

namespace GitLurker.Services
{
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

        #endregion

        #region Methods

        public Task AddNugetAsync(string nugetPath)
        {
            var settings = new SettingsFile();
            settings.Initialize();

            if (!settings.HasNugetSource())
            {
                return Task.CompletedTask;
            }

            var command = $@"{NugetAddCommand} ""{nugetPath}"" -source {settings.Entity.NugetSource}";
            return ExecuteCommandAsync(command, true);
        }

        #endregion
    }
}
