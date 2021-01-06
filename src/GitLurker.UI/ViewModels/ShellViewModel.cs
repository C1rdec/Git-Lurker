namespace GitLurker.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase
    {
        public ShellViewModel()
        {

        }

        #region Properties

        public string Title => ".NET 5!!!";

        #endregion
    }
}
