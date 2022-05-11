using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace GitLurker.UI.ViewModels
{
    public class ActionBarViewModel : PropertyChangedBase
    {
        #region Constructors

        public ActionBarViewModel()
        {
            Actions = new ObservableCollection<ActionViewModel>();
            Actions.Add(new ActionViewModel());
            Actions.Add(new ActionViewModel());
        }

        #endregion

        #region Properties

        public ObservableCollection<ActionViewModel> Actions { get; set; }

        #endregion
    }
}
