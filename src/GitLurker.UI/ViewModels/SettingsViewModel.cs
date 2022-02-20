using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.Services;

namespace GitLurker.UI.ViewModels
{
    public class SettingsViewModel
    {
        private System.Action _onSave;
        private SettingsFile _settingsFile;

        public SettingsViewModel(System.Action onSave)
        {
            _onSave = onSave;
            _settingsFile = new SettingsFile();
            _settingsFile.Initialize();

            RepoManager = new RepoManagerViewModel(_settingsFile);
            HotkeyViewModel = new HotkeyViewModel(_settingsFile.Entity.HotKey, Save);
            IoC.Get<DialogService>().Register(this);
        }

        #region Properties

        public RepoManagerViewModel RepoManager { get; set; }

        public HotkeyViewModel HotkeyViewModel { get; set; }

        #endregion

        #region Methods

        public void Save()
        {
            _settingsFile.Save();
            _onSave?.Invoke();
        }

        #endregion
    }
}
