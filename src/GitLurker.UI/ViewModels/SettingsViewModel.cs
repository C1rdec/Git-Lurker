﻿using System;
using GitLurker.Models;

namespace GitLurker.UI.ViewModels
{
    public class SettingsViewModel
    {
        private Action _onSave;
        private SettingsFile _settingsFile;

        public SettingsViewModel(Action onSave)
        {
            _onSave = onSave;
            _settingsFile = new SettingsFile();
            _settingsFile.Initialize();

            RepoManager = new RepoManagerViewModel(_settingsFile);
            HotkeyViewModel = new HotkeyViewModel(_settingsFile.Entity.HotKey, Save);
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
