using System.Collections.ObjectModel;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.UI.Services;

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
            _file.OnFileSaved += File_OnFileSaved;
            Actions = new ObservableCollection<CustomActionTileViewModel>();
            AddActions();
        }

        private void File_OnFileSaved(object sender, System.EventArgs e)
        {
            foreach (var action in Actions)
            {
                action.PropertyChanged -= CustomActionManagerViewModel_PropertyChanged;
            }

            Actions.Clear();


            AddActions();
        }

        #endregion

        #region Properties

        public ObservableCollection<CustomActionTileViewModel> Actions { get; set; }

        #endregion

        #region Methods

        public void Add()
        {
            var viewModel = new CustomActionViewModel(new CustomAction(), true);
            IoC.Get<FlyoutService>().Show("New Action", viewModel, MahApps.Metro.Controls.Position.Right);
        }

        private void CustomActionManagerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var viewModel = sender as CustomActionTileViewModel;
            if (viewModel == null)
            {
                return;
            }

            if (e.PropertyName == "Deleted")
            {
                viewModel.PropertyChanged -= CustomActionManagerViewModel_PropertyChanged;
                Actions.Remove(viewModel);
            }
        }

        private void AddActions()
        {
            foreach (var action in _file.Entity.Actions)
            {
                var viewModel = new CustomActionTileViewModel(action);
                viewModel.PropertyChanged += CustomActionManagerViewModel_PropertyChanged;
                Actions.Add(viewModel);
            }
        }

        #endregion
    }
}
