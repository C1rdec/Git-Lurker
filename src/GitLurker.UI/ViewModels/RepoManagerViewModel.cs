using System.Collections.ObjectModel;
using GitLurker.Models;

namespace GitLurker.UI.ViewModels
{
    public class RepoManagerViewModel
    {
        private SettingsFile _settings;

        public RepoManagerViewModel(SettingsFile settings)
        {
            _settings = settings;

            Paths = new ObservableCollection<string>();
            foreach (var workspace in _settings.Entity.Workspaces)
            {
                Paths.Add(workspace);
            }
        }

        public ObservableCollection<string> Paths { get; set; }
    }
}
