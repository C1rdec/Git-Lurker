﻿using System.Collections.Generic;
using System.Linq;
using AppDataFileManager;

namespace GitLurker.Models
{
    public class CustomActionSettingsFile : AppDataFileBase<CustomActionSettings>
    {
        #region Properties

        protected override string FileName => "Actions.json";

        protected override string FolderName => "GitLurker";

        #endregion

        #region Methods

        public void AddAction(CustomAction action)
        {
            Entity.Actions.Add(action);
            Save();
        }

        public void DeleteAction(CustomAction action)
        {
            Entity.Actions.Remove(action);
            Save();
        }

        public IEnumerable<CustomAction> GetActions(string path) => Entity.Actions.Where(a => a.Repositories.Contains(path));

        #endregion
    }
}
