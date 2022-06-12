using System.Collections.ObjectModel;
using Caliburn.Micro;
using GitLurker.Models;

namespace GitLurker.UI.ViewModels
{
    public class CustomActionManagerViewModel : PropertyChangedBase
    {
        #region Fields

        private CustomActionSettingsFile _file;

        #endregion

        #region Constructors

        public CustomActionManagerViewModel()
        {
            _file = IoC.Get<CustomActionSettingsFile>();
            Actions = new ObservableCollection<CustomActionTileViewModel>();
            foreach (var action in _file.Entity.Actions)
            {
                Actions.Add(new CustomActionTileViewModel(action));
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<CustomActionTileViewModel> Actions { get; set; }

        #endregion
    }
}
