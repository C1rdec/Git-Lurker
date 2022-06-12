using System.Collections.Generic;

namespace GitLurker.Models
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
