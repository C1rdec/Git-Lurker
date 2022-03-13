using System;
using System.IO;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.Services;

namespace GitLurker.UI.ViewModels
{
    public class SettingsViewModel
    {
        #region Fields

        private System.Action _onSave;
        private SettingsFile _settingsFile;

        #endregion

        #region Constructors

        public SettingsViewModel(System.Action onSave)
        {
            _onSave = onSave;
            _settingsFile = IoC.Get<SettingsFile>();
            _settingsFile.Initialize();

            RepoManager = new RepoManagerViewModel(_settingsFile);
            HotkeyViewModel = new HotkeyViewModel(_settingsFile.Entity.HotKey, Save);
            IoC.Get<DialogService>().Register(this);
        }

        #endregion

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
