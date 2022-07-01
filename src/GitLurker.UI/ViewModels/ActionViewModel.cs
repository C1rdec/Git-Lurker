using System;
using System.Threading.Tasks;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace GitLurker.UI.ViewModels
{
    public class ActionViewModel
    {
        #region Fields

        private Func<Task> _action;
        private PackIconControlBase _icon;
        private bool _permanent;
        private Brush _brush;

        #endregion

        #region Constructors

        public ActionViewModel(Func<Task> action, PackIconControlBase icon)
            : this(action, icon, true)
        {
        }

        public ActionViewModel(Func<Task> action, PackIconControlBase icon, bool permanent)
        {
            _action = action;

            icon.Height = 30;
            icon.Width = 30;
            _icon = icon;
            _permanent = permanent;
        }

        #endregion

        #region Properties

        public PackIconControlBase Icon => _icon;

        public bool Permanent => _permanent;

        #endregion

        #region Methods

        public async void OnClick() => await _action();

        #endregion
    }
}
