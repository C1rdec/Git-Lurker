using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.UI.Services;
using MahApps.Metro.IconPacks;

namespace GitLurker.UI.ViewModels
{
    public class CustomActionTileViewModel : PropertyChangedBase
    {
        #region Fields

        private CustomAction _action;

        #endregion

        #region Constructors

        public CustomActionTileViewModel(CustomAction action)
        {
            _action = action;
        }

        #endregion

        #region Properties

        public string ActionName => _action.Name;

        public PackIconControlBase Icon => new PackIconMaterial() { Kind = System.Enum.Parse<PackIconMaterialKind>(_action.Icon) };

        #endregion

        #region Methods

        public void Open()
        {
            IoC.Get<FlyoutService>().Show(_action.Name, new CustomActionViewModel(_action), MahApps.Metro.Controls.Position.Right);
        }

        #endregion
    }
}
