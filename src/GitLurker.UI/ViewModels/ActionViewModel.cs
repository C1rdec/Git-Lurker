using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using MahApps.Metro.IconPacks;

namespace GitLurker.UI.ViewModels
{
    public class ActionViewModel : PropertyChangedBase
    {
        #region Fields

        private Guid _id;
        private Func<Task> _action;
        private PackIconControlBase _icon;
        private bool _permanent;
        private bool _isDisable;
        private bool _isActive;

        #endregion

        #region Constructors

        public ActionViewModel(Func<Task> action, PackIconControlBase icon, bool permanent, Guid id)
        {
            _action = action;

            icon.Height = 30;
            icon.Width = 30;
            _icon = icon;
            _permanent = permanent;
            _id = id;
        }

        #endregion

        #region Properties

        public Guid Id => _id;

        public PackIconControlBase Icon => _icon;

        public bool Permanent => _permanent;

        public bool IsEnable => !IsDisable;

        public bool IsDisable
        {
            get => _isDisable;
            set
            {
                _isDisable = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsEnable);
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        public async void OnClick() 
        {
            await _action();
        }

        #endregion
    }
}
