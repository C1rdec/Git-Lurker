﻿using System;

namespace GitLurker.UI.ViewModels
{
    public class FolderViewModel
    {
        #region Fields

        private string _folder;
        private Action<string> _deleteCallback;

        #endregion

        #region Constructors

        public FolderViewModel(string folder, Action<string> deleteCallback)
        {
            _folder = folder;
            _deleteCallback = deleteCallback;
        }

        #endregion

        #region Properties

        public string Folder => _folder;

        #endregion

        #region Methods

        public void Delete() => _deleteCallback(_folder);

        #endregion
    }
}
