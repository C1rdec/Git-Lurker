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

            Folders = new ObservableCollection<FolderViewModel>();
            foreach (var workspace in _settings.Entity.Workspaces)
            {
                Folders.Add(new FolderViewModel(workspace));
            }
        }

        public ObservableCollection<FolderViewModel> Folders { get; set; }
    }
}
