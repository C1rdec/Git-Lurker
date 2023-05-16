using System.Collections.Generic;

namespace GitLurker.Core.Models
{
    public class CustomActionSettings
    {
        #region Constructors

        public CustomActionSettings()
        {
            Actions = new List<CustomAction>();
        }

        #endregion

        #region Properties

        public List<CustomAction> Actions { get; set; }

        #endregion
    }
}
