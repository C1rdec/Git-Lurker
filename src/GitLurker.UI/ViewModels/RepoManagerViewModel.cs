using System.Collections.ObjectModel;
using System.Windows.Forms;
using Caliburn.Micro;
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
                Folders.Add(new FolderViewModel(workspace));
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

            var settings = IoC.Get<SettingsFile>();
            settings.Entity.Workspaces.Add(dialog.SelectedPath);
            settings.Save();

            Folders.Add(new FolderViewModel(dialog.SelectedPath));
        }

        #endregion
    }
}
