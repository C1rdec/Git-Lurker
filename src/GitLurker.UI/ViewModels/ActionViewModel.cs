using System;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;

namespace GitLurker.UI.ViewModels
{
    public class ActionViewModel
    {
        #region Fields

        private Func<Task> _action;
        private PackIconControlBase _icon;

        #endregion

        #region Constructors

        public ActionViewModel(Func<Task> action, PackIconControlBase icon)
        {
            _action = action;

            icon.Height = 30;
            icon.Width = 30;
            _icon = icon;
        }

        #endregion

        #region Properties

        public PackIconControlBase Icon => _icon;

        #endregion

        #region Methods

        public async void OnClick() => await _action();

        #endregion
    }
}
