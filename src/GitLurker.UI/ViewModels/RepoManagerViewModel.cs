using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using GitLurker.Models;

namespace GitLurker.UI.ViewModels
{
    public class RepoManagerViewModel
    {
        #region Fields

        private SettingsFile _settings;

        #endregion

        #region Constructors

        public RepoManagerViewModel(SettingsFile settings)
        {
            _settings = settings;

            Folders = new ObservableCollection<FolderViewModel>();
            foreach (var workspace in _settings.Entity.Workspaces)
            {
                Folders.Add(new FolderViewModel(workspace, Delete));
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<FolderViewModel> Folders { get; set; }

        #endregion

        #region Methods

        public void Add()
        {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            var existingWorkspace = _settings.Entity.Workspaces.FirstOrDefault(w => w == dialog.SelectedPath);
            if (existingWorkspace != null)
            {
                return;
            }

            _settings.Entity.Workspaces.Add(dialog.SelectedPath);
            _settings.Save();

            Folders.Add(new FolderViewModel(dialog.SelectedPath, Delete));
        }

        private void Delete(string folderPath)
        {
            var folder = Folders.FirstOrDefault(f => f.Folder == folderPath);
            if (folder != null)
            {
                Folders.Remove(folder);
                _settings.RemoveWorkspace(folderPath);
            }
        }

        #endregion
    }
}
